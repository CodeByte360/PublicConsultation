
window.secugen = {
    capture: async function () {
        try {
            // SecuGen WebAPI default URL
            const url = "http://localhost:8000/api/capture";
            const response = await fetch(url, {
                method: "GET", // Some versions use GET, some POST
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                throw new Error("Failed to communicate with SecuGen WebAPI");
            }

            const data = await response.json();
            if (data.ErrorCode === 0) {
                return {
                    success: true,
                    image: data.Base64Image,
                    template: data.Base64Template
                };
            } else {
                return {
                    success: false,
                    error: "ErrorCode: " + data.ErrorCode
                };
            }
        } catch (error) {
            console.error("SecuGen Capture Error:", error);
            return {
                success: false,
                error: error.message || "Is SecuGen WebAPI running?"
            };
        }
    },
    match: async function (template1, template2) {
        try {
            const url = "http://localhost:8000/api/match";
            const response = await fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
                body: JSON.stringify({
                    Template1: template1,
                    Template2: template2
                })
            });

            if (!response.ok) {
                throw new Error("Failed to communicate with SecuGen WebAPI for matching");
            }

            const data = await response.json();
            if (data.ErrorCode === 0) {
                return {
                    success: true,
                    score: data.MatchingScore,
                    isMatch: data.MatchingScore >= 60 // Threshold can be adjusted
                };
            } else {
                return {
                    success: false,
                    error: "ErrorCode: " + data.ErrorCode
                };
            }
        } catch (error) {
            console.error("SecuGen Match Error:", error);
            return {
                success: false,
                error: error.message || "Is SecuGen WebAPI running?"
            };
        }
    }
};
