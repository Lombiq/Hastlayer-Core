using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Moq
{
    public static class MoqExtensions
    {
        public static void ForceMock<T>(this Mock<T> mock, IServiceCollection services) where T : class
        {
            services.RemoveImplementations<T>();
            services.AddSingleton(mock.Object);
        }
    }
}
