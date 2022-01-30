using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NetCoreApi.Controllers.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebAPIAutores.Tests.Mocks;

namespace WebAPIAutores.Tests.PruebasUnitarias
{
    [TestClass]
    public class RootControllerTests
    {
        [TestMethod]
        public async Task SiUsuarioEsAdmin_Obtenemos4Links()
        {
            // Preparación
            var authorizationService = new AuthorizationServiceMock();
            authorizationService.Resultado = AuthorizationResult.Success();
            var rootController = new RootController(authorizationService);
            rootController.Url = new URLHelperMock();

            // Ejecución
            var resultado = await rootController.Get();

            // Verificación
            Assert.AreEqual(4, resultado.Value.Count());
        }

        [TestMethod]
        public async Task SiUsuarioNoEsAdmin_Obtenemos2Links()
        {
            // Preparación
            var authorizationService = new AuthorizationServiceMock();
            authorizationService.Resultado = AuthorizationResult.Failed();
            var rootController = new RootController(authorizationService);
            rootController.Url = new URLHelperMock();

            // Ejecución
            var resultado = await rootController.Get();

            // Verificación
            Assert.AreEqual(2, resultado.Value.Count());

        }

        [TestMethod]
        public async Task SiUsuarioNoEsAdmin_Obtenemos2Links_UsandoMoq()
        {
            // Preparación
            var mockAuthorizationService = new Mock<IAuthorizationService>();
            
            //configuración del IAuthorizacionService y el retorno
            mockAuthorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()
                )).Returns(Task.FromResult(AuthorizationResult.Failed()));

            //IAuthorizationService: hay dos métodos 
            mockAuthorizationService.Setup(x => x.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<string>()
            )).Returns(Task.FromResult(AuthorizationResult.Failed()));


            var mockURLHelper = new Mock<IUrlHelper>();
            mockURLHelper.Setup(x =>
                                    x.Link(It.IsAny<string>(),
                                    It.IsAny<object>()))
                                    .Returns(string.Empty);

            // Object: instancia una interfaz con el objeto
            var rootController = new RootController(mockAuthorizationService.Object);
            rootController.Url = mockURLHelper.Object;

            // Ejecución
            var resultado = await rootController.Get();

            // Verificación
            Assert.AreEqual(2, resultado.Value.Count());

        }
    }
}
