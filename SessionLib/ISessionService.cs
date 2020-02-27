using SharedModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SessionLib
{
    public interface ISessionService
    {
        Task<string> Create(ICollection<SessionKeyJsonValue> sessionValues);
        Task<ICollection<SessionKeyJsonValue>> Get(string key);
        Task Save(string key, ICollection<SessionKeyJsonValue> sessionValues);
        Task Delete(string key);
    }
}
