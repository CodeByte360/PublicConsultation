
window.secugen = {
    capture: async function () {
        try
        {
            // Standard SecuGen WebAPI URL with parameters
            let url = "https://localhost:8443/SGIFPCapture?Timeout=10000&Quality=50&TemplateFormat=ISO";

            const response = await fetch(url, {
                method: "GET",
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                throw new Error("Failed to communicate with SecuGen WebAPI (HTTP " + response.status + ")");
            }

            const data = await response.json();

            // map standard fields (BMPBase64) or fallback to previous (Base64Image) just in case
            if (data.ErrorCode === 0) {
                return {
                    success: true,
                    image: data.BMPBase64 || data.Base64Image,
                    template: data.TemplateBase64 || data.Base64Template
                };
            } else {
                return {
                    success: false,
                    error: "Device Error: " + (data.ErrorDescription || "ErrorCode: " + data.ErrorCode)
                };
            }
        } catch (error) {
            console.error("SecuGen Capture Error:", error);
            let msg = error.message || "Failed to fetch";
            if (msg.includes("Failed to fetch")) {
                msg = "Connection Refused. SecuGen WebAPI may not be running or certificate is not accepted.\n" +
                    "Please open https://localhost:8443/SGIFPCapture in a new tab and accept the certificate.";
            }
            return {
                success: false,
                error: msg
            };
        }
    },
    match: async function (template1, template2) {
        try {
            // Standard endpoint for matching is SGIMatchScore
            const url = "https://localhost:8443/SGIMatchScore";

            // SGIMatchScore usually takes form data or query params, but some versions accept JSON. 
            // Let's try standard query params approach as it's most robust for GET, 
            // but for POST we often send form-urlencoded.
            // However, WebAPI documentation says POST with JSON or Form. 
            // Previous code used POST JSON. Let's stick to that but update endpoint.

            const response = await fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded", // Safe bet for legacy
                },
                body: `Execute=Match&Template1=${encodeURIComponent(template1)}&Template2=${encodeURIComponent(template2)}&TemplateFormat=ISO`
            });
            // NOTE: If JSON is preferred by installed version, we might need to revert to JSON. 
            // But usually WebAPI uses GET/POST with simple params. 
            // Actually, let's look at the standard again. 
            // SGIMatchScore usually accepts parameters in URL or body.

            if (!response.ok) {
                throw new Error("Failed to communicate with SecuGen WebAPI for matching");
            }

            const data = await response.json();
            if (data.ErrorCode === 0) {
                return {
                    success: true,
                    score: data.MatchingScore,
                    isMatch: data.MatchingScore >= 100 // Standard threshold is typically 100+ for high security, but matching depends on config.
                };
            } else {
                return {
                    success: false,
                    error: "ErrorCode: " + data.ErrorCode
                };
            }
        } catch (error) {
            console.error("SecuGen Match Error:", error);
            let msg = error.message || "Failed to fetch";
            if (msg.includes("Failed to fetch")) {
                msg = "Connection Refused. Please open https://localhost:8443/SGIFPCapture to accept certificate.";
            }
            return {
                success: false,
                error: msg
            };
        }
    },
    submitForm: function (id) {
        var form = document.getElementById(id);
        if (form) {
            form.submit();
            return true;
        }
        return false;
    }
};
