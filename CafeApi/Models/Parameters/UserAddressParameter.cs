﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using static CafeApi.Models.Enums.UserAddressEnum;

namespace CafeApi.Models.Parameters
{
    public class UserAddressParameter
    {
        public List<string>? Address { get; set; }
        public List<double>? Latitude { get; set; } 
        public List<double>? Longitude { get; set; }
        [EnumDataType(typeof(UserAddressStatus))]
        public List<UserAddressStatus>? Status { get; set; }
    }
}
