using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcService.Samples.Services
{
    public class CatService : Cat.CatBase
    {
        private readonly ILogger<CatService> _logger;
        public CatService(ILogger<CatService> logger)
        {
            _logger = logger;
        }

        public override Task<CatReply> GetCatName(CatRequest request, ServerCallContext context)
        {
            return Task.FromResult(new CatReply
            {
                Message = "Hello " + request.Name
            });
        }
    }
}

