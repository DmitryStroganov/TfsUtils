namespace TfsUtils.Common.Model
{
    public interface ITfsUtil
    {
    }

    public interface ITfsUtil<in T> : ITfsUtil
    {
        void Invoke(params T[] args);

        bool ValidateArguments(params T[] args);
    }
}