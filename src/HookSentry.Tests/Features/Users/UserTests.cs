using HookSentry.Api.Features.Users.Domain;

namespace HookSentry.Tests.Features.Users;

public class UserTests
{
    private static readonly Guid ValidTenantId = Guid.NewGuid();
    private const string ValidEmail = "joao@exemplo.com";
    private const string ValidHash = "salt123:hash456";

    public class Construtor
    {
        [Fact]
        public void Deve_Gerar_Id_Nao_Vazio()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            Assert.NotEqual(Guid.Empty, user.Id);
        }

        [Fact]
        public void Dois_Users_Devem_Ter_Ids_Diferentes()
        {
            var a = new User(ValidTenantId, "a@exemplo.com", ValidHash);
            var b = new User(ValidTenantId, "b@exemplo.com", ValidHash);

            Assert.NotEqual(a.Id, b.Id);
        }

        [Fact]
        public void Deve_Atribuir_TenantId_Informado()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            Assert.Equal(ValidTenantId, user.TenantId);
        }

        [Fact]
        public void Deve_Normalizar_Email_Para_Lowercase_Na_Criacao()
        {
            var user = new User(ValidTenantId, "Joao@EXEMPLO.COM", ValidHash);

            Assert.Equal("joao@exemplo.com", user.Email);
        }

        [Fact]
        public void Deve_Atribuir_PasswordHash_Informado()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            Assert.Equal(ValidHash, user.PasswordHash);
        }

        [Fact]
        public void Role_Deve_Ser_Developer_Por_Padrao()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            Assert.Equal(UserRole.Developer, user.Role);
        }

        [Fact]
        public void Deve_Aceitar_Role_Admin()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash, UserRole.Admin);

            Assert.Equal(UserRole.Admin, user.Role);
        }

        [Fact]
        public void Status_Deve_Ser_Active_Na_Criacao()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            Assert.Equal(UserStatus.Active, user.Status);
        }

        [Fact]
        public void CreatedAt_Deve_Ser_Definido_Como_UtcNow()
        {
            var antes = DateTimeOffset.UtcNow;
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            Assert.True(user.CreatedAt >= antes);
        }

        [Fact]
        public void UpdatedAt_Deve_Ser_Definido_Como_UtcNow()
        {
            var antes = DateTimeOffset.UtcNow;
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            Assert.True(user.UpdatedAt >= antes);
        }

        [Fact]
        public void CreatedAt_E_UpdatedAt_Devem_Ser_Iguais_Na_Criacao()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            Assert.Equal(user.CreatedAt, user.UpdatedAt);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_TenantId_For_Vazio()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => new User(Guid.Empty, ValidEmail, ValidHash));

            Assert.Contains("TenantId", ex.Message);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_Email_For_Nulo()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => new User(ValidTenantId, null!, ValidHash));

            Assert.Contains("Email", ex.Message);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_Email_For_Vazio()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => new User(ValidTenantId, "", ValidHash));

            Assert.Contains("Email", ex.Message);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_Email_For_Apenas_Espacos()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => new User(ValidTenantId, "   ", ValidHash));

            Assert.Contains("Email", ex.Message);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_Email_Nao_Contiver_Arroba()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => new User(ValidTenantId, "emailsemarroba.com", ValidHash));

            Assert.Contains("Email", ex.Message);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_Email_Ultrapassar_255_Chars()
        {
            var emailLongo = new string('a', 250) + "@x.com";

            var ex = Assert.Throws<ArgumentException>(
                () => new User(ValidTenantId, emailLongo, ValidHash));

            Assert.Contains("255", ex.Message);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_PasswordHash_For_Nulo()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => new User(ValidTenantId, ValidEmail, null!));

            Assert.Contains("PasswordHash", ex.Message);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_PasswordHash_For_Vazio()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => new User(ValidTenantId, ValidEmail, ""));

            Assert.Contains("PasswordHash", ex.Message);
        }
    }

    public class MetodoSetEmail
    {
        [Fact]
        public void Deve_Atualizar_Email()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            user.SetEmail("novo@exemplo.com");

            Assert.Equal("novo@exemplo.com", user.Email);
        }

        [Fact]
        public void Deve_Normalizar_Email_Para_Lowercase()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            user.SetEmail("NOVO@EXEMPLO.COM");

            Assert.Equal("novo@exemplo.com", user.Email);
        }

        [Fact]
        public void Deve_Remover_Espacos_Do_Email()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            user.SetEmail("  novo@exemplo.com  ");

            Assert.Equal("novo@exemplo.com", user.Email);
        }

        [Fact]
        public void Deve_Atualizar_UpdatedAt()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);
            Thread.Sleep(20);
            var antes = DateTimeOffset.UtcNow;

            user.SetEmail("novo@exemplo.com");

            Assert.True(user.UpdatedAt >= antes);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);
            var createdAtOriginal = user.CreatedAt;
            Thread.Sleep(20);

            user.SetEmail("novo@exemplo.com");

            Assert.Equal(createdAtOriginal, user.CreatedAt);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_Email_For_Nulo()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            Assert.Throws<ArgumentException>(() => user.SetEmail(null!));
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_Email_For_Vazio()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            Assert.Throws<ArgumentException>(() => user.SetEmail(""));
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_Email_Nao_Contiver_Arroba()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            Assert.Throws<ArgumentException>(() => user.SetEmail("invalido.com"));
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_Email_Ultrapassar_255_Chars()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);
            var emailLongo = new string('a', 250) + "@x.com";

            Assert.Throws<ArgumentException>(() => user.SetEmail(emailLongo));
        }
    }

    public class MetodoSetPasswordHash
    {
        [Fact]
        public void Deve_Atualizar_PasswordHash()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            user.SetPasswordHash("novoSalt:novoHash");

            Assert.Equal("novoSalt:novoHash", user.PasswordHash);
        }

        [Fact]
        public void Deve_Atualizar_UpdatedAt()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);
            Thread.Sleep(20);
            var antes = DateTimeOffset.UtcNow;

            user.SetPasswordHash("novoSalt:novoHash");

            Assert.True(user.UpdatedAt >= antes);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);
            var createdAtOriginal = user.CreatedAt;
            Thread.Sleep(20);

            user.SetPasswordHash("novoSalt:novoHash");

            Assert.Equal(createdAtOriginal, user.CreatedAt);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_PasswordHash_For_Nulo()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            Assert.Throws<ArgumentException>(() => user.SetPasswordHash(null!));
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_PasswordHash_For_Vazio()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            Assert.Throws<ArgumentException>(() => user.SetPasswordHash(""));
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_PasswordHash_For_Apenas_Espacos()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            Assert.Throws<ArgumentException>(() => user.SetPasswordHash("   "));
        }
    }

    public class MetodoSetRole
    {
        [Fact]
        public void Deve_Atualizar_Role_Para_Admin()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            user.SetRole(UserRole.Admin);

            Assert.Equal(UserRole.Admin, user.Role);
        }

        [Fact]
        public void Deve_Manter_Role_Developer()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash, UserRole.Admin);

            user.SetRole(UserRole.Developer);

            Assert.Equal(UserRole.Developer, user.Role);
        }

        [Fact]
        public void Deve_Atualizar_UpdatedAt()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);
            Thread.Sleep(20);
            var antes = DateTimeOffset.UtcNow;

            user.SetRole(UserRole.Admin);

            Assert.True(user.UpdatedAt >= antes);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);
            var createdAtOriginal = user.CreatedAt;
            Thread.Sleep(20);

            user.SetRole(UserRole.Admin);

            Assert.Equal(createdAtOriginal, user.CreatedAt);
        }

        [Fact]
        public void Deve_Lancar_Excecao_Quando_Role_For_Invalido()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);
            var roleInvalido = (UserRole)99;

            Assert.Throws<ArgumentOutOfRangeException>(() => user.SetRole(roleInvalido));
        }
    }

    public class MetodoActivate
    {
        [Fact]
        public void Deve_Definir_Status_Como_Active()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);
            user.Deactivate();

            user.Activate();

            Assert.Equal(UserStatus.Active, user.Status);
        }

        [Fact]
        public void Deve_Ser_Idempotente_Se_Ja_Estiver_Active()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            user.Activate();

            Assert.Equal(UserStatus.Active, user.Status);
        }

        [Fact]
        public void Deve_Atualizar_UpdatedAt()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);
            user.Deactivate();
            Thread.Sleep(20);
            var antes = DateTimeOffset.UtcNow;

            user.Activate();

            Assert.True(user.UpdatedAt >= antes);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);
            var createdAtOriginal = user.CreatedAt;
            user.Deactivate();
            Thread.Sleep(20);

            user.Activate();

            Assert.Equal(createdAtOriginal, user.CreatedAt);
        }
    }

    public class MetodoDeactivate
    {
        [Fact]
        public void Deve_Definir_Status_Como_Inactive()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);

            user.Deactivate();

            Assert.Equal(UserStatus.Inactive, user.Status);
        }

        [Fact]
        public void Deve_Ser_Idempotente_Se_Ja_Estiver_Inactive()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);
            user.Deactivate();

            user.Deactivate();

            Assert.Equal(UserStatus.Inactive, user.Status);
        }

        [Fact]
        public void Deve_Atualizar_UpdatedAt()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);
            Thread.Sleep(20);
            var antes = DateTimeOffset.UtcNow;

            user.Deactivate();

            Assert.True(user.UpdatedAt >= antes);
        }

        [Fact]
        public void Nao_Deve_Alterar_CreatedAt()
        {
            var user = new User(ValidTenantId, ValidEmail, ValidHash);
            var createdAtOriginal = user.CreatedAt;
            Thread.Sleep(20);

            user.Deactivate();

            Assert.Equal(createdAtOriginal, user.CreatedAt);
        }
    }
}
