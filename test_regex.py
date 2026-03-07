import requests

url = "http://localhost:5000/analyze_sentiment"
test_cases = [
    "Na . ami ei Rulu er sathe ekmot noi",
    "Ami ei niyam er sathe ekmat noi",
    "kono dorkar nai",
    "Bhalo initiative, thik ache",
    "sotik na",
    "[email protected]"
]

for text in test_cases:
    try:
        response = requests.post(url, json={"text": text}, timeout=10)
        result = response.json()
        print(f"Text: {text} -> Result: {result['sentiment']} ({result['probability']:.2f})")
    except Exception as e:
        print(f"Error: {e}")
