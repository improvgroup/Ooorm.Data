using System.Threading.Tasks;

namespace Ooorm.Data.Core
{
    public interface IdConvertable<TId>
    {
        bool HasValue { get; }

        TId ToId();

        /// <summary>
        /// Gets ref result without knowledge of generic type
        /// </summary>
        /// <returns></returns>
        Task<object> GetObject();
    }
}
