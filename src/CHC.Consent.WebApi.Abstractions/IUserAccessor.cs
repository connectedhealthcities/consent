﻿using CHC.Consent.Security;

namespace CHC.Consent.WebApi.Abstractions
{
    public interface IUserAccessor
    {
        IUser GetUser();
    }
}