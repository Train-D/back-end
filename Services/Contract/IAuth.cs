using Train_D.Models;
using Train_D.Models.AuthenticationModels;

namespace Train_D.Services
{
    public interface IAuth
    {
        public Task<AuthModel> Register(RegisterModel model);
        public Task<AuthModel> Login(LoginModel model);
        public Task<string> AddRole(AddRoleModel model);
        public Task<AuthModel> LoginGoogle(string credential);
        public Task<ProfileModel> GetDataForProfile(string UserName);
        public User UpdateDataForProfile(User user);
        public Task<User> GetUser(string UserName);
    }

}
