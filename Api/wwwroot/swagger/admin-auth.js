(function () {
    const originalFetch = window.fetch.bind(window);

    function readPersistedBearerToken() {
        const raw = window.localStorage.getItem("authorized");
        if (!raw) {
            return null;
        }

        try {
            const authorized = JSON.parse(raw);
            const bearer = authorized.Bearer || authorized.bearer;
            return bearer && (bearer.value || bearer.token);
        } catch {
            return null;
        }
    }

    window.fetch = function (input, init) {
        const requestUrl = typeof input === "string" ? input : input.url;
        const url = new URL(requestUrl, window.location.href);

        if (url.origin !== window.location.origin || !url.pathname.endsWith("/swagger.json")) {
            return originalFetch(input, init);
        }

        const token = readPersistedBearerToken();
        if (!token) {
            return originalFetch(input, init);
        }

        const headers = new Headers(input instanceof Request ? input.headers : init && init.headers);
        headers.set("Authorization", token.startsWith("Bearer ") ? token : `Bearer ${token}`);

        if (input instanceof Request) {
            return originalFetch(new Request(input, { ...(init || {}), headers }));
        }

        return originalFetch(input, { ...(init || {}), headers });
    };
})();
