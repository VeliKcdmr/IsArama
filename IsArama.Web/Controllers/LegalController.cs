using Microsoft.AspNetCore.Mvc;

namespace IsArama.Web.Controllers;

public class LegalController : Controller
{
    [Route("gizlilik-politikasi")]
    public IActionResult GizlilikPolitikasi() => View();

    [Route("kullanim-kosullari")]
    public IActionResult KullanimKosullari() => View();

    [Route("kvkk")]
    public IActionResult Kvkk() => View();

    [Route("cerez-politikasi")]
    public IActionResult CerezPolitikasi() => View();
}
