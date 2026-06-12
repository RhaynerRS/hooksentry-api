using HookSentry.Domain.Common;
using HookSentry.Domain.Senders;

namespace HookSentry.Tests.Features.Senders;

public class WebhookSenderTests
{
    private static readonly Guid ValidDestinationId = Guid.NewGuid();
    private static readonly Guid ValidTenantId = Guid.NewGuid();

    public class Construtor
    {
        [Fact]
        public void Deve_Gerar_Id_Nao_Vazio()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);

            Assert.NotEqual(Guid.Empty, sender.Id);
        }

        [Fact]
        public void Deve_Atribuir_DestinationId()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);

            Assert.Equal(ValidDestinationId, sender.DestinationId);
        }

        [Fact]
        public void Deve_Atribuir_TenantId()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);

            Assert.Equal(ValidTenantId, sender.TenantId);
        }

        [Fact]
        public void Deve_Atribuir_Label_Quando_Fornecido()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId, "GitHub Webhooks");

            Assert.Equal("GitHub Webhooks", sender.Label);
        }

        [Fact]
        public void Label_Deve_Ser_Nulo_Quando_Nao_Fornecido()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);

            Assert.Null(sender.Label);
        }

        [Fact]
        public void Mapping_Deve_Ser_Nulo_Por_Padrao()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);

            Assert.Null(sender.Mapping);
        }

        [Fact]
        public void IngestTokenHash_Deve_Ser_Nulo_Por_Padrao()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);

            Assert.Null(sender.IngestTokenHash);
        }

        [Fact]
        public void Deve_Setar_CreatedAt_E_UpdatedAt()
        {
            var antes = DateTimeOffset.UtcNow;
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);

            Assert.True(sender.CreatedAt >= antes);
            Assert.True(sender.UpdatedAt >= antes);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_DestinationId_For_Vazio()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new WebhookSender(Guid.Empty, ValidTenantId));

            Assert.Contains("DestinationId", ex.Message);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_TenantId_For_Vazio()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new WebhookSender(ValidDestinationId, Guid.Empty));

            Assert.Contains("TenantId", ex.Message);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_Label_Exceder_255_Chars()
        {
            var labelLongo = new string('x', 256);

            var ex = Assert.Throws<ArgumentException>(() =>
                new WebhookSender(ValidDestinationId, ValidTenantId, labelLongo));

            Assert.Contains("Label", ex.Message);
        }
    }

    public class MetodoSetLabel
    {
        [Fact]
        public void Deve_Atualizar_Label()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);

            sender.SetLabel("Stripe Payments");

            Assert.Equal("Stripe Payments", sender.Label);
        }

        [Fact]
        public void Deve_Permitir_Label_Nulo()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId, "label");

            sender.SetLabel(null);

            Assert.Null(sender.Label);
        }

        [Fact]
        public void Deve_Atualizar_UpdatedAt()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);
            Thread.Sleep(20);
            var antes = DateTimeOffset.UtcNow;

            sender.SetLabel("novo label");

            Assert.True(sender.UpdatedAt >= antes);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);
            var createdAtOriginal = sender.CreatedAt;
            Thread.Sleep(20);

            sender.SetLabel("novo label");

            Assert.Equal(createdAtOriginal, sender.CreatedAt);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_Label_Exceder_255_Chars()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);
            var labelLongo = new string('x', 256);

            var ex = Assert.Throws<ArgumentException>(() => sender.SetLabel(labelLongo));

            Assert.Contains("Label", ex.Message);
        }
    }

    public class MetodoRotateIngestToken
    {
        [Fact]
        public void Deve_Retornar_Token_Nao_Vazio()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);

            var token = sender.RotateIngestToken();

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void Token_Deve_Comecar_Com_Prefixo_Sndr()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);

            var token = sender.RotateIngestToken();

            Assert.StartsWith(IngestToken.SenderPrefix, token);
        }

        [Fact]
        public void Token_Retornado_Deve_Ter_69_Chars()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);

            var token = sender.RotateIngestToken();

            Assert.Equal(69, token.Length);
        }

        [Fact]
        public void Deve_Setar_IngestTokenHash()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);

            sender.RotateIngestToken();

            Assert.False(string.IsNullOrWhiteSpace(sender.IngestTokenHash));
        }

        [Fact]
        public void IngestTokenHash_Deve_Ser_Diferente_Do_Token_Bruto()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);

            var rawToken = sender.RotateIngestToken();

            Assert.NotEqual(rawToken, sender.IngestTokenHash);
        }

        [Fact]
        public void IngestTokenHash_Deve_Ser_SHA256_Do_Token_Bruto()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);

            var rawToken = sender.RotateIngestToken();

            Assert.Equal(IngestToken.Hash(rawToken), sender.IngestTokenHash);
        }

        [Fact]
        public void Dois_Tokens_Devem_Ser_Diferentes()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);

            var token1 = sender.RotateIngestToken();
            var token2 = sender.RotateIngestToken();

            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void Segunda_Chamada_Deve_Atualizar_Hash()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);
            sender.RotateIngestToken();
            var hashAnterior = sender.IngestTokenHash;

            sender.RotateIngestToken();

            Assert.NotEqual(hashAnterior, sender.IngestTokenHash);
        }

        [Fact]
        public void Deve_Atualizar_UpdatedAt()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);
            Thread.Sleep(20);
            var antes = DateTimeOffset.UtcNow;

            sender.RotateIngestToken();

            Assert.True(sender.UpdatedAt >= antes);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);
            var createdAtOriginal = sender.CreatedAt;
            Thread.Sleep(20);

            sender.RotateIngestToken();

            Assert.Equal(createdAtOriginal, sender.CreatedAt);
        }
    }

    public class MetodoSetMapping
    {
        private const string ValidMapping = """{"campo": "id"}""";

        [Fact]
        public void Deve_Atribuir_Mapping()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);

            sender.SetMapping(ValidMapping);

            Assert.Equal(ValidMapping, sender.Mapping);
        }

        [Fact]
        public void Deve_Permitir_Mapping_Nulo()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);
            sender.SetMapping(ValidMapping);

            sender.SetMapping(null);

            Assert.Null(sender.Mapping);
        }

        [Fact]
        public void Deve_Atualizar_UpdatedAt()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);
            Thread.Sleep(20);
            var antes = DateTimeOffset.UtcNow;

            sender.SetMapping(ValidMapping);

            Assert.True(sender.UpdatedAt >= antes);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var sender = new WebhookSender(ValidDestinationId, ValidTenantId);
            var createdAtOriginal = sender.CreatedAt;
            Thread.Sleep(20);

            sender.SetMapping(ValidMapping);

            Assert.Equal(createdAtOriginal, sender.CreatedAt);
        }
    }
}
