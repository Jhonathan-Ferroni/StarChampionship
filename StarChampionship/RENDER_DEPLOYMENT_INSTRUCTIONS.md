# 🚀 INSTRUÇÕES RENDER - LOGIN JWT

## ✅ Tudo foi commitado no GitHub!

Os arquivos novos já foram enviados para:
```
https://github.com/Jhonathan-Ferroni/StarChampionship
```

---

## 📋 O que fazer no Render

### 1️⃣ **Forçar Redeploy**

Acesse seu dashboard no Render:
```
https://dashboard.render.com/
```

Encontre o serviço **StarChampionship**:
- Clique em **Settings** (ou acesse direto o serviço)
- Procure por **"Deploy"** ou **"Redeploy"**
- Clique em **"Redeploy latest commit"**

Ou use o git:
```sh
git commit --allow-empty -m "Force Render redeploy"
git push origin main
```

### 2️⃣ **Verificar Variáveis de Ambiente**

No Render Dashboard, vá para:
**Settings → Environment Variables**

Verifique se tem:
```
JWT_SECRET_KEY=SuaChaveSecretaComPeloMenos32Caracteres
ADMIN_PASSWORD=SuaSenhaDeAdminSegura
JWT_ISSUER=StarChampionshipApi
JWT_AUDIENCE=StarChampionshipUsers
JWT_EXPIRATION_MINUTES=60
ASPNETCORE_ENVIRONMENT=Production
```

❌ **Se faltar alguma** → Adicione!  
✅ **Se tem todas** → Próximo passo

### 3️⃣ **Esperar Deploy Completar**

O Render vai:
1. Clonar o código do GitHub
2. Fazer build (`dotnet build`)
3. Rodar a aplicação
4. ~2-3 minutos

Você pode acompanhar em **"Logs"**

### 4️⃣ **Testar o Login**

Acesse:
```
https://starchampionship.onrender.com/admin/login
```

Digite:
- **Senha:** A mesma que configurou em `ADMIN_PASSWORD` no Render

Clique em **"Entrar"**

---

## ✅ Arquivos Novos no GitHub

```
✅ wwwroot/js/auth-service.js         ← Gerencia JWT
✅ wwwroot/js/auth-check.js           ← Protege páginas
✅ Pages/AdminLogin.cshtml            ← Página de login
✅ Pages/AdminLogin.cshtml.cs         ← Code-behind
✅ Views/Shared/_Layout.cshtml        ← Scripts adicionados
```

---

## 🔄 Se o Render não Reconhecer

### Opção A: Force Redeploy pelo Render

1. Dashboard Render
2. Seu serviço
3. **"Redeploy latest commit"**
4. Esperar 2-3 minutos

### Opção B: Force via Git

```sh
git commit --allow-empty -m "Redeploy frontend JWT"
git push origin main
```

Render vai detectar novo commit e fazer redeploy automático.

### Opção C: Verificar Logs

Se der erro, veja os logs:
1. Dashboard Render
2. Seu serviço
3. Aba **"Logs"**
4. Procure por erros

---

## 🧪 Teste Final

### 1. Acesse
```
https://starchampionship.onrender.com/admin/login
```

### 2. Veja uma página bonita com:
```
🔒 StarChampionship
Admin Access - Gerenciamento de Jogadores

[Campo de Senha]
[Botão Entrar]
```

### 3. Digite a senha do admin

### 4. Clique em "Entrar"

### 5. Deve redirecionar para `/players`

### 6. Clique em "Editar" um jogador

### 7. ✅ Deve abrir a página de edição

---

## ❌ Se Ainda Tiver Erro 401

### Verificar:

1. **A senha está correta?**
   - Render → Settings → Environment Variables
   - Verificar `ADMIN_PASSWORD`

2. **As variáveis JWT estão configuradas?**
   - Render → Settings → Environment Variables
   - Devem ter: `JWT_SECRET_KEY`, `JWT_ISSUER`, etc

3. **O código foi atualizado?**
   - Render fez o redeploy?
   - Limpar cache do navegador (Ctrl+Shift+Delete)
   - Tentar em navegador privado

4. **Console do navegador tem erros?**
   - F12 → Console
   - Procurar por erros em vermelho

---

## 📞 Checklist Final

- [ ] Redeploy iniciado no Render
- [ ] Logs mostram sucesso (sem erros)
- [ ] Acesso `/admin/login` abriu página bonita
- [ ] Digite senha do admin
- [ ] Clicou em "Entrar"
- [ ] Redirecionou para `/players`
- [ ] Clicou em "Editar" jogador
- [ ] Página de edição abriu (sem erro 401) ✅

---

## 🎯 Status

```
✅ Código commitado no GitHub
✅ Arquivos novos adicionados
✅ Pronto para Render fazer deploy
```

**Próximo passo:** Faça redeploy no Render!

---

**Resultado esperado:**
- Login funciona ✅
- Edição de jogadores funciona ✅
- Sistema retorna ao normal ✅
