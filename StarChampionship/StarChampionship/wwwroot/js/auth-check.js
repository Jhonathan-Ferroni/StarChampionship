(function(){
    const protectedPrefixes = ['/players/create','/players/edit','/players/delete','/admin'];
    const path = window.location.pathname.toLowerCase();
    if(protectedPrefixes.some(p => path.startsWith(p))){
        if(!window.authService || !window.authService.isAuthenticated()){
            window.location.href = '/admin/login';
        }
    }
})();
