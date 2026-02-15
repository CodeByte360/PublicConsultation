from flask import Flask, request, jsonify
from vaderSentiment.vaderSentiment import SentimentIntensityAnalyzer
from sklearn.feature_extraction.text import TfidfVectorizer
import numpy as np
from sumy.parsers.plaintext import PlaintextParser
from sumy.nlp.tokenizers import Tokenizer
from sumy.summarizers.lsa import LsaSummarizer
import nltk

app = Flask(__name__)
analyzer = SentimentIntensityAnalyzer()

# Initialize sumy resources (extractive summarization)
try:
    nltk.data.find('tokenizers/punkt')
except LookupError:
    nltk.download('punkt')

def get_professional_score(text):
    # VADER analysis
    scores = analyzer.polarity_scores(text)
    compound = scores['compound']
    
    # Professional thresholds
    if compound >= 0.05:
        sentiment = "Positive"
        # Map -1..1 to 0..1 probability-ish score for UI consistency
        probability = 0.5 + (compound / 2)
    elif compound <= -0.05:
        sentiment = "Negative"
        probability = 0.5 + (abs(compound) / 2)
    else:
        sentiment = "Neutral"
        probability = 0.0

    return sentiment, probability, compound

@app.route('/analyze_sentiment', methods=['POST'])
def analyze_sentiment():
    content = request.json
    text = content.get('text', '')
    
    if not text:
        return jsonify({'sentiment': 'Neutral', 'probability': 0.0})
    
    sentiment, prob, _ = get_professional_score(text)
    
    return jsonify({
        'sentiment': sentiment,
        'probability': float(prob)
    })

@app.route('/summarize', methods=['POST'])
def summarize():
    content = request.json
    text = content.get('text', '')
    
    if not text or len(text) < 20:
        return jsonify({'summary': text})
    
    try:
        parser = PlaintextParser.from_string(text, Tokenizer("english"))
        summarizer = LsaSummarizer()
        # Summarize to 1-2 most significant sentences
        summary_sentences = summarizer(parser.document, 2)
        summary = " ".join([str(sentence) for sentence in summary_sentences])
        
        if not summary:
            summary = text[:200] + "..." if len(text) > 200 else text
            
        return jsonify({'summary': summary})
    except Exception as e:
        return jsonify({'summary': text[:200] + "...", 'error': str(e)})

@app.route('/analyze_batch', methods=['POST'])
def analyze_batch():
    content = request.json
    texts = content.get('texts', [])
    
    if not texts:
        return jsonify({'results': []})
    
    # 1. Sentiment Analysis per item
    results = []
    compound_scores = []
    
    for text in texts:
        sentiment, _, compound = get_professional_score(text)
        results.append({
            'text': text,
            'sentiment': sentiment
        })
        compound_scores.append(compound)
        
    # 2. Professional Theme Extraction using Scikit-Learn TF-IDF
    # We ignore common English words and look for significant terms
    themes = []
    if len(texts) > 2: # Need some data to extract patterns
        try:
            vectorizer = TfidfVectorizer(stop_words='english', max_features=5, ngram_range=(1, 2))
            tfidf_matrix = vectorizer.fit_transform(texts)
            feature_names = vectorizer.get_feature_names_out()
            
            # Sum tfidf frequency of each term across documents
            dense = tfidf_matrix.todense()
            denselist = dense.tolist()
            if denselist:
                df_sum = np.sum(denselist, axis=0)
                # Get top indices
                top_indices = df_sum.argsort().tolist()[0][::-1][:5]
                themes = [feature_names[i] for i in top_indices]
        except:
             # Fallback if too little data or error
             pass

    # Risk / Sensitivity Analysis
    # Check if there are strongly negative sentiments
    avg_compound = sum(compound_scores) / len(compound_scores) if compound_scores else 0
    sensitivity_alert = avg_compound < -0.3 

    return jsonify({
        'results': results,
        'extracted_themes': themes,
        'sensitivity_alert': sensitivity_alert
    })

if __name__ == '__main__':
    # Run on port 5000
    print("Professional AI Service (VADER + Scikit-Learn) Running...")
    app.run(host='0.0.0.0', port=5000)
