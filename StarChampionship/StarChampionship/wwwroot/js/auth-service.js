class AuthService {
    constructor(){
        this.apiBase = '/api';
        this.tokenKey = 'jwt_token';
        this.expKey = 'jwt_token_expiry';
    }

    async login(password){
        try{
            const resp = await fetch(this.apiBase + '/auth/login',{
                method: 'POST',
                headers: { 'Content-Type':'application/json' },
                body: JSON.stringify({ password })
            });

            if(!resp.ok){
                const txt = await resp.text();
                return { success:false, error: txt };
            }

            const json = await resp.json();
            if(json && json.token){
                localStorage.setItem(this.tokenKey,json.token);
                if(json.expiresAt) localStorage.setItem(this.expKey,json.expiresAt);
                return { success:true, token:json.token };
            }

            return { success:false, error:'Invalid response' };
        }catch(err){
            return { success:false, error: err.message };
        }
    }

    getToken(){
        return localStorage.getItem(this.tokenKey);
    }

    isAuthenticated(){
        const t = this.getToken();
        return !!t;
    }

    async fetchWithAuth(path, options={}){
        const headers = options.headers || {};
        const token = this.getToken();
        if(token) headers['Authorization'] = 'Bearer ' + token;
        options.headers = headers;
        return fetch(path, options);
    }

    logout(){
        localStorage.removeItem(this.tokenKey);
        localStorage.removeItem(this.expKey);
    }
}

window.authService = new AuthService();
