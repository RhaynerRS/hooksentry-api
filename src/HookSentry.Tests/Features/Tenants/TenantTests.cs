using HookSentry.Api.Features.Tenants.Domain;

namespace HookSentry.Tests.Features.Tenants;

public class TenantTests
{
    public class Construtor
    {
        [Fact]
        public void Deve_Gerar_Id_Nao_Vazio()
        {
            var tenant = new Tenant("Acme");

            Assert.NotEqual(Guid.Empty, tenant.Id);
        }

        [Fact]
        public void Dois_Tenants_Devem_Ter_Ids_Diferentes()
        {
            var a = new Tenant("Acme A");
            var b = new Tenant("Acme B");

            Assert.NotEqual(a.Id, b.Id);
        }

        [Fact]
        public void Deve_Atribuir_Name_Informado()
        {
            var tenant = new Tenant("Acme");

            Assert.Equal("Acme", tenant.Name);
        }

        [Fact]
        public void MaxTrys_Deve_Ser_10_Por_Padrao()
        {
            var tenant = new Tenant("Acme");

            Assert.Equal(10, tenant.MaxTrys);
        }

        [Fact]
        public void CircuitBreakerTimer_Deve_Ser_300_Por_Padrao()
        {
            var tenant = new Tenant("Acme");

            Assert.Equal(300, tenant.CircuitBreakerTimer);
        }

        [Fact]
        public void Deve_Aceitar_MaxTrys_Customizado()
        {
            var tenant = new Tenant("Acme", maxTrys: 5);

            Assert.Equal(5, tenant.MaxTrys);
        }

        [Fact]
        public void Deve_Aceitar_CircuitBreakerTimer_Customizado()
        {
            var tenant = new Tenant("Acme", circuitBreakerTimer: 600);

            Assert.Equal(600, tenant.CircuitBreakerTimer);
        }

        [Fact]
        public void Deve_Gerar_WebhookSecret_Na_Criacao()
        {
            var tenant = new Tenant("Acme");

            Assert.False(string.IsNullOrWhiteSpace(tenant.WebhookSecret));
        }

        [Fact]
        public void WebhookSecret_Deve_Ser_Hexadecimal_Lowercase_De_64_Chars()
        {
            var tenant = new Tenant("Acme");

            Assert.Equal(64, tenant.WebhookSecret.Length);
            Assert.Matches("^[0-9a-f]{64}$", tenant.WebhookSecret);
        }

        [Fact]
        public void Dois_Tenants_Devem_Ter_WebhookSecrets_Diferentes()
        {
            var a = new Tenant("Acme A");
            var b = new Tenant("Acme B");

            Assert.NotEqual(a.WebhookSecret, b.WebhookSecret);
        }

        [Fact]
        public void CreatedAt_Deve_Ser_Definido_Como_UtcNow()
        {
            var antes = DateTimeOffset.UtcNow;
            var tenant = new Tenant("Acme");

            Assert.True(tenant.CreatedAt >= antes);
        }

        [Fact]
        public void UpdatedAt_Deve_Ser_Definido_Como_UtcNow()
        {
            var antes = DateTimeOffset.UtcNow;
            var tenant = new Tenant("Acme");

            Assert.True(tenant.UpdatedAt >= antes);
        }

        [Fact]
        public void CreatedAt_E_UpdatedAt_Devem_Ser_Iguais_Na_Criacao()
        {
            var tenant = new Tenant("Acme");

            Assert.Equal(tenant.CreatedAt, tenant.UpdatedAt);
        }
    }

    public class MetodoUpdateSettings
    {
        [Fact]
        public void Deve_Atualizar_MaxTrys()
        {
            var tenant = new Tenant("Acme");

            tenant.UpdateSettings(maxTrys: 5, circuitBreakerTimer: 300);

            Assert.Equal(5, tenant.MaxTrys);
        }

        [Fact]
        public void Deve_Atualizar_CircuitBreakerTimer()
        {
            var tenant = new Tenant("Acme");

            tenant.UpdateSettings(maxTrys: 10, circuitBreakerTimer: 600);

            Assert.Equal(600, tenant.CircuitBreakerTimer);
        }

        [Fact]
        public void Deve_Atualizar_UpdatedAt()
        {
            var tenant = new Tenant("Acme");
            Thread.Sleep(20);
            var antes = DateTimeOffset.UtcNow;

            tenant.UpdateSettings(maxTrys: 5, circuitBreakerTimer: 300);

            Assert.True(tenant.UpdatedAt >= antes);
        }

        [Fact]
        public void Nao_Deve_Alterar_Name()
        {
            var tenant = new Tenant("Acme");

            tenant.UpdateSettings(maxTrys: 5, circuitBreakerTimer: 300);

            Assert.Equal("Acme", tenant.Name);
        }

        [Fact]
        public void Nao_Deve_Alterar_Id()
        {
            var tenant = new Tenant("Acme");
            var idOriginal = tenant.Id;

            tenant.UpdateSettings(maxTrys: 5, circuitBreakerTimer: 300);

            Assert.Equal(idOriginal, tenant.Id);
        }

        [Fact]
        public void Nao_Deve_Alterar_WebhookSecret()
        {
            var tenant = new Tenant("Acme");
            var secretOriginal = tenant.WebhookSecret;

            tenant.UpdateSettings(maxTrys: 5, circuitBreakerTimer: 300);

            Assert.Equal(secretOriginal, tenant.WebhookSecret);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var tenant = new Tenant("Acme");
            var createdAtOriginal = tenant.CreatedAt;
            Thread.Sleep(20);

            tenant.UpdateSettings(maxTrys: 5, circuitBreakerTimer: 300);

            Assert.Equal(createdAtOriginal, tenant.CreatedAt);
        }
    }

    public class MetodoRotateWebhookSecret
    {
        [Fact]
        public void Deve_Gerar_Novo_WebhookSecret_Diferente_Do_Anterior()
        {
            var tenant = new Tenant("Acme");
            var secretOriginal = tenant.WebhookSecret;

            tenant.RotateWebhookSecret();

            Assert.NotEqual(secretOriginal, tenant.WebhookSecret);
        }

        [Fact]
        public void Novo_WebhookSecret_Deve_Ser_Hexadecimal_Lowercase_De_64_Chars()
        {
            var tenant = new Tenant("Acme");

            tenant.RotateWebhookSecret();

            Assert.Equal(64, tenant.WebhookSecret.Length);
            Assert.Matches("^[0-9a-f]{64}$", tenant.WebhookSecret);
        }

        [Fact]
        public void Deve_Atualizar_UpdatedAt()
        {
            var tenant = new Tenant("Acme");
            Thread.Sleep(20);
            var antes = DateTimeOffset.UtcNow;

            tenant.RotateWebhookSecret();

            Assert.True(tenant.UpdatedAt >= antes);
        }

        [Fact]
        public void Nao_Deve_Alterar_Name()
        {
            var tenant = new Tenant("Acme");

            tenant.RotateWebhookSecret();

            Assert.Equal("Acme", tenant.Name);
        }

        [Fact]
        public void Nao_Deve_Alterar_Id()
        {
            var tenant = new Tenant("Acme");
            var idOriginal = tenant.Id;

            tenant.RotateWebhookSecret();

            Assert.Equal(idOriginal, tenant.Id);
        }

        [Fact]
        public void Nao_Deve_Alterar_MaxTrys()
        {
            var tenant = new Tenant("Acme");

            tenant.RotateWebhookSecret();

            Assert.Equal(10, tenant.MaxTrys);
        }

        [Fact]
        public void Nao_Deve_Alterar_CircuitBreakerTimer()
        {
            var tenant = new Tenant("Acme");

            tenant.RotateWebhookSecret();

            Assert.Equal(300, tenant.CircuitBreakerTimer);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var tenant = new Tenant("Acme");
            var createdAtOriginal = tenant.CreatedAt;
            Thread.Sleep(20);

            tenant.RotateWebhookSecret();

            Assert.Equal(createdAtOriginal, tenant.CreatedAt);
        }
    }
}
