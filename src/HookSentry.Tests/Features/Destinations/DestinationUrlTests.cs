using HookSentry.Api.Features.Destinations.Domain;

namespace HookSentry.Tests.Features.Destinations;

public class DestinationUrlTests
{
    private static readonly Guid ValidTenantId = Guid.NewGuid();
    private const string ValidUrl = "https://example.com/webhook";

    public class Construtor
    {
        [Fact]
        public void Deve_Gerar_Id_Nao_Vazio()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            Assert.NotEqual(Guid.Empty, dest.Id);
        }

        [Fact]
        public void Dois_Destinos_Devem_Ter_Ids_Diferentes()
        {
            var a = new DestinationUrl(ValidTenantId, ValidUrl);
            var b = new DestinationUrl(ValidTenantId, ValidUrl);

            Assert.NotEqual(a.Id, b.Id);
        }

        [Fact]
        public void Deve_Atribuir_TenantId_Informado()
        {
            var tenantId = Guid.NewGuid();
            var dest = new DestinationUrl(tenantId, ValidUrl);

            Assert.Equal(tenantId, dest.TenantId);
        }

        [Fact]
        public void Deve_Atribuir_Url_Informada()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            Assert.Equal(ValidUrl, dest.Url);
        }

        [Fact]
        public void Status_Deve_Ser_Active_Por_Padrao()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            Assert.Equal(DestinationUrlStatus.Active, dest.Status);
        }

        [Fact]
        public void ServerRateLimit_Deve_Ser_5_Por_Padrao()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            Assert.Equal(5, dest.ServerRateLimit);
        }

        [Fact]
        public void Deve_Aceitar_ServerRateLimit_Customizado()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl, serverRateLimit: 10);

            Assert.Equal(10, dest.ServerRateLimit);
        }

        [Fact]
        public void CreatedAt_Deve_Ser_Definido_Como_UtcNow()
        {
            var antes = DateTimeOffset.UtcNow;
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            Assert.True(dest.CreatedAt >= antes);
        }

        [Fact]
        public void UpdatedAt_Deve_Ser_Definido_Como_UtcNow()
        {
            var antes = DateTimeOffset.UtcNow;
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            Assert.True(dest.UpdatedAt >= antes);
        }

        [Fact]
        public void CreatedAt_E_UpdatedAt_Devem_Ser_Iguais_Na_Criacao()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            Assert.Equal(dest.CreatedAt, dest.UpdatedAt);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_TenantId_For_Vazio()
        {
            Assert.Throws<ArgumentException>(() =>
                new DestinationUrl(Guid.Empty, ValidUrl));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Deve_Lancar_Excecao_Quando_Url_For_Nula_Ou_Vazia(string? url)
        {
            Assert.Throws<ArgumentException>(() =>
                new DestinationUrl(ValidTenantId, url!));
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_Url_For_Http()
        {
            Assert.Throws<ArgumentException>(() =>
                new DestinationUrl(ValidTenantId, "http://example.com/webhook"));
        }

        [Theory]
        [InlineData("ftp://example.com")]
        [InlineData("not-a-url")]
        [InlineData("example.com")]
        public void Deve_Lancar_Excecao_Quando_Url_For_Invalida(string url)
        {
            Assert.Throws<ArgumentException>(() =>
                new DestinationUrl(ValidTenantId, url));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Deve_Lancar_Excecao_Quando_ServerRateLimit_For_Menor_Que_1(int limit)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DestinationUrl(ValidTenantId, ValidUrl, serverRateLimit: limit));
        }
    }

    public class MetodoIsActive
    {
        [Fact]
        public void Deve_Retornar_True_Quando_Status_For_Active()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            Assert.True(dest.IsActive());
        }

        [Fact]
        public void Deve_Retornar_False_Quando_Status_For_Inactive()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            dest.Deactivate();

            Assert.False(dest.IsActive());
        }

        [Fact]
        public void Deve_Retornar_False_Quando_Status_For_Suspended()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            dest.Suspend();

            Assert.False(dest.IsActive());
        }
    }

    public class MetodoActivate
    {
        [Fact]
        public void Deve_Definir_Status_Como_Active()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            dest.Deactivate();

            dest.Activate();

            Assert.Equal(DestinationUrlStatus.Active, dest.Status);
        }

        [Fact]
        public void Deve_Atualizar_UpdatedAt()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            dest.Deactivate();
            Thread.Sleep(20);
            var antes = DateTimeOffset.UtcNow;

            dest.Activate();

            Assert.True(dest.UpdatedAt >= antes);
        }

        [Fact]
        public void Nao_Deve_Alterar_Id()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            var idOriginal = dest.Id;

            dest.Activate();

            Assert.Equal(idOriginal, dest.Id);
        }

        [Fact]
        public void Nao_Deve_Alterar_TenantId()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.Activate();

            Assert.Equal(ValidTenantId, dest.TenantId);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            var createdAtOriginal = dest.CreatedAt;
            Thread.Sleep(20);

            dest.Activate();

            Assert.Equal(createdAtOriginal, dest.CreatedAt);
        }
    }

    public class MetodoDeactivate
    {
        [Fact]
        public void Deve_Definir_Status_Como_Inactive()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.Deactivate();

            Assert.Equal(DestinationUrlStatus.Inactive, dest.Status);
        }

        [Fact]
        public void Deve_Atualizar_UpdatedAt()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            Thread.Sleep(20);
            var antes = DateTimeOffset.UtcNow;

            dest.Deactivate();

            Assert.True(dest.UpdatedAt >= antes);
        }

        [Fact]
        public void IsActive_Deve_Retornar_False_Apos_Deactivate()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.Deactivate();

            Assert.False(dest.IsActive());
        }

        [Fact]
        public void Nao_Deve_Alterar_Url()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.Deactivate();

            Assert.Equal(ValidUrl, dest.Url);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            var createdAtOriginal = dest.CreatedAt;
            Thread.Sleep(20);

            dest.Deactivate();

            Assert.Equal(createdAtOriginal, dest.CreatedAt);
        }
    }

    public class MetodoSuspend
    {
        [Fact]
        public void Deve_Definir_Status_Como_Suspended()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.Suspend();

            Assert.Equal(DestinationUrlStatus.Suspended, dest.Status);
        }

        [Fact]
        public void Deve_Atualizar_UpdatedAt()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            Thread.Sleep(20);
            var antes = DateTimeOffset.UtcNow;

            dest.Suspend();

            Assert.True(dest.UpdatedAt >= antes);
        }

        [Fact]
        public void IsActive_Deve_Retornar_False_Apos_Suspend()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.Suspend();

            Assert.False(dest.IsActive());
        }

        [Fact]
        public void Nao_Deve_Alterar_Url()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.Suspend();

            Assert.Equal(ValidUrl, dest.Url);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            var createdAtOriginal = dest.CreatedAt;
            Thread.Sleep(20);

            dest.Suspend();

            Assert.Equal(createdAtOriginal, dest.CreatedAt);
        }

        [Fact]
        public void Activate_Apos_Suspend_Deve_Reabrir_Destino()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            dest.Suspend();

            dest.Activate();

            Assert.True(dest.IsActive());
        }
    }

    public class MetodoSetTenantId
    {
        [Fact]
        public void Deve_Atualizar_TenantId()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            var novoTenantId = Guid.NewGuid();

            dest.SetTenantId(novoTenantId);

            Assert.Equal(novoTenantId, dest.TenantId);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_TenantId_For_Vazio()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            Assert.Throws<ArgumentException>(() => dest.SetTenantId(Guid.Empty));
        }

        [Fact]
        public void Nao_Deve_Alterar_Status()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.SetTenantId(Guid.NewGuid());

            Assert.Equal(DestinationUrlStatus.Active, dest.Status);
        }

        [Fact]
        public void Nao_Deve_Alterar_Url()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.SetTenantId(Guid.NewGuid());

            Assert.Equal(ValidUrl, dest.Url);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            var createdAtOriginal = dest.CreatedAt;
            Thread.Sleep(20);

            dest.SetTenantId(Guid.NewGuid());

            Assert.Equal(createdAtOriginal, dest.CreatedAt);
        }
    }

    public class MetodoSetUrl
    {
        [Fact]
        public void Deve_Atualizar_Url()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            var novaUrl = "https://novo-endpoint.com/hook";

            dest.SetUrl(novaUrl);

            Assert.Equal(novaUrl, dest.Url);
        }

        [Fact]
        public void Deve_Atualizar_UpdatedAt()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            Thread.Sleep(20);
            var antes = DateTimeOffset.UtcNow;

            dest.SetUrl("https://novo-endpoint.com/hook");

            Assert.True(dest.UpdatedAt >= antes);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Deve_Lancar_Excecao_Quando_Url_For_Nula_Ou_Vazia(string? url)
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            Assert.Throws<ArgumentException>(() => dest.SetUrl(url!));
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_Url_For_Http()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            Assert.Throws<ArgumentException>(() => dest.SetUrl("http://example.com/hook"));
        }

        [Fact]
        public void Nao_Deve_Alterar_Status()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.SetUrl("https://novo-endpoint.com/hook");

            Assert.Equal(DestinationUrlStatus.Active, dest.Status);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            var createdAtOriginal = dest.CreatedAt;
            Thread.Sleep(20);

            dest.SetUrl("https://novo-endpoint.com/hook");

            Assert.Equal(createdAtOriginal, dest.CreatedAt);
        }
    }

    public class MetodoSetServerRateLimit
    {
        [Fact]
        public void Deve_Atualizar_ServerRateLimit()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.SetServerRateLimit(20);

            Assert.Equal(20, dest.ServerRateLimit);
        }

        [Fact]
        public void Deve_Atualizar_UpdatedAt()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            Thread.Sleep(20);
            var antes = DateTimeOffset.UtcNow;

            dest.SetServerRateLimit(20);

            Assert.True(dest.UpdatedAt >= antes);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-50)]
        public void Deve_Lancar_Excecao_Quando_Limit_For_Menor_Que_1(int limit)
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            Assert.Throws<ArgumentOutOfRangeException>(() => dest.SetServerRateLimit(limit));
        }

        [Fact]
        public void Deve_Aceitar_Limit_Igual_A_1()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.SetServerRateLimit(1);

            Assert.Equal(1, dest.ServerRateLimit);
        }

        [Fact]
        public void Nao_Deve_Alterar_Status()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.SetServerRateLimit(20);

            Assert.Equal(DestinationUrlStatus.Active, dest.Status);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            var createdAtOriginal = dest.CreatedAt;
            Thread.Sleep(20);

            dest.SetServerRateLimit(20);

            Assert.Equal(createdAtOriginal, dest.CreatedAt);
        }
    }

    public class MetodoSetAuth
    {
        private const string ValidEncrypted = "credencial-criptografada";

        [Fact]
        public void Deve_Atribuir_AuthType_E_Credenciais()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.SetAuth(DestinationAuthType.ApiKey, ValidEncrypted);

            Assert.Equal(DestinationAuthType.ApiKey, dest.AuthType);
            Assert.Equal(ValidEncrypted, dest.CredentialsEncrypted);
        }

        [Theory]
        [InlineData(DestinationAuthType.ApiKey)]
        [InlineData(DestinationAuthType.BearerToken)]
        [InlineData(DestinationAuthType.JwtBearer)]
        [InlineData(DestinationAuthType.BasicAuth)]
        public void Deve_Aceitar_Todos_Os_Tipos_De_Auth(DestinationAuthType authType)
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.SetAuth(authType, ValidEncrypted);

            Assert.Equal(authType, dest.AuthType);
        }

        [Fact]
        public void Deve_Permitir_Remover_Auth_Com_Null_Null()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            dest.SetAuth(DestinationAuthType.BearerToken, ValidEncrypted);

            dest.SetAuth(null, null);

            Assert.Null(dest.AuthType);
            Assert.Null(dest.CredentialsEncrypted);
        }

        [Fact]
        public void Deve_Atualizar_UpdatedAt()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            Thread.Sleep(20);
            var antes = DateTimeOffset.UtcNow;

            dest.SetAuth(DestinationAuthType.BasicAuth, ValidEncrypted);

            Assert.True(dest.UpdatedAt >= antes);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);
            var createdAtOriginal = dest.CreatedAt;
            Thread.Sleep(20);

            dest.SetAuth(DestinationAuthType.BearerToken, ValidEncrypted);

            Assert.Equal(createdAtOriginal, dest.CreatedAt);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_AuthType_Definido_Sem_Credenciais()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            Assert.Throws<ArgumentException>(() =>
                dest.SetAuth(DestinationAuthType.ApiKey, null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Deve_Lancar_Excecao_Quando_Credenciais_Forem_Vazias_Com_AuthType(string credencial)
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            Assert.Throws<ArgumentException>(() =>
                dest.SetAuth(DestinationAuthType.ApiKey, credencial));
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_Credenciais_Fornecidas_Sem_AuthType()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            Assert.Throws<ArgumentException>(() =>
                dest.SetAuth(null, ValidEncrypted));
        }

        [Fact]
        public void Nao_Deve_Alterar_Url()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.SetAuth(DestinationAuthType.BasicAuth, ValidEncrypted);

            Assert.Equal(ValidUrl, dest.Url);
        }

        [Fact]
        public void Nao_Deve_Alterar_Status()
        {
            var dest = new DestinationUrl(ValidTenantId, ValidUrl);

            dest.SetAuth(DestinationAuthType.JwtBearer, ValidEncrypted);

            Assert.Equal(DestinationUrlStatus.Active, dest.Status);
        }
    }
}
