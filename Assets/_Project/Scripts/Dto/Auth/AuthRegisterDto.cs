using System;

namespace VRCollege.Net.Dto
{
    [Serializable]
    public sealed class AuthRegisterDto
    {
        public string login;
        public string password;
        public string name;
        public int specialtyId;
    }
}