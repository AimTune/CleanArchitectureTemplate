using Shared.Errors;

namespace API.Errors;

public static class ODataBaseErrors
{
    public static Error CantAdd => Error.Failure("ODataBase.CantAdd", "Bu adres ekleme iþlemine izin vermemektedir!");
    public static Error CantUpdate => Error.Failure("ODataBase.CantUpdate", "Bu adres güncelleme iþlemine izin vermemektedir!");
}