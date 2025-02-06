using Haondt.Web.Core.Services;

namespace GabbroSecretManager.UI.Shared.Middlewares
{
    public interface ISpecificExceptionActionResultFactory : IExceptionActionResultFactory
    {
        public bool CanHandle(Exception exception);
    }
}
