﻿using Train_D.Models;

namespace Train_D.Services
{
    public interface IAuth
    {
        public Task<AuthModel> Register(RegisterModel model);
        public Task<AuthModel> Login(LoginModel model);
    }
}
