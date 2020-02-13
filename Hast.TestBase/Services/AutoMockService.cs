using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hast.TestBase.Services
{
    public class AutoMockService
    {
        public MockBehavior Behavior { get; set; }

        public AutoMockService(MockBehavior behavior)
        {
            Behavior = behavior;
        }
    }
}
