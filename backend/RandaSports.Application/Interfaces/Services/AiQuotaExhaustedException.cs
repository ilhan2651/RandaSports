using System;
using System.Collections.Generic;
using System.Text;

namespace RandaSports.Application.Interfaces.Services
{

    /// <summary>
    /// Sağlayıcının kotası dolduğunda fırlatılır. Çağıran taraf bunu gördüğünde
    /// deneme hakkını yakmamalı: istekte bir sorun yok, sistem meşgul.
    /// </summary>
    public sealed class AiQuotaExhaustedException(string message, Exception? inner = null) : Exception(message, inner);

}
