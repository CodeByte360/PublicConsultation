from flask import Flask, request, jsonify
from transformers import pipeline
from sklearn.feature_extraction.text import TfidfVectorizer
import numpy as np
from sumy.parsers.plaintext import PlaintextParser
from sumy.nlp.tokenizers import Tokenizer
from sumy.summarizers.lsa import LsaSummarizer
import nltk
import re

app = Flask(__name__)

# Initialize Multilingual Sentiment Analysis (DistilBERT)
# This model is more stable and supports English, Bengali, and many others.
sentiment_task = pipeline("sentiment-analysis", model="lxyuan/distilbert-base-multilingual-cased-sentiments-student")

# Map model labels to professional sentiment names
label_mapping = {
    "positive": "Positive",
    "neutral": "Neutral",
    "negative": "Negative"
}

# Extend VADER lexicon for Romanized Bengali (Mixed Words)
# This is now handled by the Multilingual Transformer above.

# Initialize sumy resources (extractive summarization)
try:
    nltk.data.find('tokenizers/punkt')
except LookupError:
    nltk.download('punkt')

def get_professional_score(text):
    # Perform Multilingual Transformer analysis
    try:
        lower_text = text.lower()
        
        # Manual check for clear Romanized Bengali negations that models sometimes miss
        negation_pattern = r'\b(na|noi|ekmot noi|ekmat noi|bhalo na|thik nai|sommot noi|manda|kharap|sotik noy|bhul|sotik na|dorkar nai|nai)\b'
        if re.search(negation_pattern, lower_text):
             return "Negative", 0.8, -0.8
             
        positive_pattern = r'\b(bhalo|thik ache|sotik|sahomot|sommot|dhonyobad|darun|shundor|valo|ekmot|ekmat)\b'
        if re.search(positive_pattern, lower_text):
             return "Positive", 0.8, 0.8

        prediction = sentiment_task(text)[0]
        label = prediction['label']
        probability = prediction['score']
        
        sentiment = label_mapping.get(label, "Neutral")
        
        # Approximate compound score for legacy batch sensitivity logic (-1 to 1)
        compound = 0.0
        if sentiment == "Positive": compound = probability
        elif sentiment == "Negative": compound = -probability
        
        return sentiment, probability, compound
    except Exception as e:
        print(f"Error in sentiment analysis: {e}")
        return "Neutral", 0.0, 0.0

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
