using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Models;

public partial class dbJoinnusContext : DbContext
{
    public dbJoinnusContext()
    {
    }

    public dbJoinnusContext(DbContextOptions<dbJoinnusContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Amigo> Amigos { get; set; }

    public virtual DbSet<Categoria> Categoria { get; set; }

    public virtual DbSet<Ciudad> Ciudad { get; set; }

    public virtual DbSet<Compra> Compras { get; set; }

    public virtual DbSet<DetalleCompra> DetalleCompras { get; set; }

    public virtual DbSet<Entrada> Entrada { get; set; }

    public virtual DbSet<Evento> Eventos { get; set; }

    public virtual DbSet<Faq> Faqs { get; set; }

    public virtual DbSet<HistorialBusqueda> HistorialBusqueda { get; set; }

    public virtual DbSet<LibroReclamacione> LibroReclamaciones { get; set; }

    public virtual DbSet<Lugar> Lugar { get; set; }

    public virtual DbSet<Notificacion> Notificacion { get; set; }

    public virtual DbSet<PoliticaLegal> PoliticaLegal { get; set; }

    public virtual DbSet<Promocion> Promocions { get; set; }

    public virtual DbSet<Sesion> Sesions { get; set; }

    public virtual DbSet<TipoEntrada> TipoEntrada { get; set; }

    public virtual DbSet<TipoUsuario> TipoUsuarios { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuarioRol> UsuarioRols { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=dbjoinnus_1");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Amigo>(entity =>
        {
            entity.HasKey(e => e.IdRelacion).HasName("PK__Amigo__D27D6AE79C25C7D6");

            entity.ToTable("Amigo");

            entity.HasIndex(e => new { e.IdUsuario, e.IdAmigo }, "UQ__Amigo__A3D4717FF8AEB080").IsUnique();

            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValue("Activo");
            entity.Property(e => e.FechaAgregado)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdAmigoNavigation).WithMany(p => p.AmigoIdAmigoNavigations)
                .HasForeignKey(d => d.IdAmigo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Amigo__IdAmigo__02084FDA");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.AmigoIdUsuarioNavigations)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Amigo__IdUsuario__02FC7413");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__Categori__A3C02A1038CAD003");

            entity.HasIndex(e => e.Nombre, "UQ__Categori__75E3EFCFD7422F53").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Ciudad>(entity =>
        {
            entity.HasKey(e => e.IdCiudad).HasName("PK__Ciudad__D4D3CCB065F06D1B");

            entity.ToTable("Ciudad");

            entity.HasIndex(e => new { e.Nombre, e.Pais }, "UQ__Ciudad__AFF610384C9ADC12").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Pais)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Perú");
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.IdCompra).HasName("PK__Compra__0A5CDB5C5158AF45");

            entity.ToTable("Compra");

            entity.Property(e => e.CodigoTransaccion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Confirmada).HasDefaultValue(false);
            entity.Property(e => e.EstadoPago)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pendiente");
            entity.Property(e => e.FechaCompra)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MontoTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Reembolsado).HasDefaultValue(false);

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.IdEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Compra__IdEvento__03F0984C");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Compras)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Compra__IdUsuari__04E4BC85");
        });

        modelBuilder.Entity<DetalleCompra>(entity =>
        {
            entity.HasKey(e => e.IdDetalle).HasName("PK__DetalleC__E43646A5076102E0");

            entity.ToTable("DetalleCompra");

            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Subtotal)
                .HasComputedColumnSql("([Cantidad]*[PrecioUnitario])", true)
                .HasColumnType("decimal(21, 2)");

            entity.HasOne(d => d.IdCompraNavigation).WithMany(p => p.DetalleCompras)
                .HasForeignKey(d => d.IdCompra)
                .HasConstraintName("FK__DetalleCo__IdCom__05D8E0BE");

            entity.HasOne(d => d.IdTipoEntradaNavigation).WithMany(p => p.DetalleCompras)
                .HasForeignKey(d => d.IdTipoEntrada)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DetalleCo__IdTip__06CD04F7");
        });

        modelBuilder.Entity<Entrada>(entity =>
        {
            entity.HasKey(e => e.IdEntrada).HasName("PK__Entrada__BB164DEAE1E1E1EE");

            entity.HasIndex(e => e.CodigoQr, "UQ__Entrada__DB31D3E6AE000452").IsUnique();

            entity.Property(e => e.AsistenteDocumento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.AsistenteEmail)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.AsistenteNombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CodigoQr)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("CodigoQR");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Activa");
            entity.Property(e => e.FechaEmision)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaUso).HasColumnType("datetime");

            entity.HasOne(d => d.IdDetalleNavigation).WithMany(p => p.Entrada)
                .HasForeignKey(d => d.IdDetalle)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Entrada__IdDetal__07C12930");
        });

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.HasKey(e => e.IdEvento).HasName("PK__Evento__034EFC042FA59A39");

            entity.ToTable("Evento");

            entity.HasIndex(e => e.UrlAmigable, "UQ__Evento__4988EEEBBDD0B12A").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Cancelado).HasDefaultValue(false);
            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.Destacado).HasDefaultValue(false);
            entity.Property(e => e.EnTendencia).HasDefaultValue(false);
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaFin).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio).HasColumnType("datetime");
            entity.Property(e => e.ImagenBanner)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ImagenPortada)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PoliticaReembolso).HasColumnType("text");
            entity.Property(e => e.Titulo)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.UrlAmigable)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Evento__IdCatego__08B54D69");

            entity.HasOne(d => d.IdLugarNavigation).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.IdLugar)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Evento__IdLugar__09A971A2");

            entity.HasOne(d => d.IdOrganizadorNavigation).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.IdOrganizador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Evento__IdOrgani__0A9D95DB");
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(e => e.IdPregunta).HasName("PK__FAQ__754EC09E11855C6E");

            entity.ToTable("FAQ");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Categoria)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Orden).HasDefaultValue(0);
            entity.Property(e => e.Pregunta)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Respuesta).HasColumnType("text");
        });

        modelBuilder.Entity<HistorialBusqueda>(entity =>
        {
            entity.HasKey(e => e.IdBusqueda).HasName("PK__Historia__3EFF2448C18EB8C8");

            entity.Property(e => e.FechaBusqueda)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ResultadosMostrados).HasDefaultValue(0);
            entity.Property(e => e.Termino)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.HistorialBusqueda)
                .HasForeignKey(d => d.IdCategoria)
                .HasConstraintName("FK__Historial__IdCat__0B91BA14");

            entity.HasOne(d => d.IdCiudadNavigation).WithMany(p => p.HistorialBusqueda)
                .HasForeignKey(d => d.IdCiudad)
                .HasConstraintName("FK__Historial__IdCiu__0C85DE4D");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.HistorialBusqueda)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Historial__IdUsu__0D7A0286");
        });

        modelBuilder.Entity<LibroReclamacione>(entity =>
        {
            entity.HasKey(e => e.IdReclamo).HasName("PK__LibroRec__19682C6610FC22A0");

            entity.Property(e => e.AdjuntosUrls)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.CorreoContacto)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Detalle).HasColumnType("text");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Recibido");
            entity.Property(e => e.FechaReclamo)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaRespuesta).HasColumnType("datetime");
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NumeroRegistro)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Respuesta).HasColumnType("text");
            entity.Property(e => e.TelefonoContacto)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TipoDocumento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TipoReclamo)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Lugar>(entity =>
        {
            entity.HasKey(e => e.IdLugar).HasName("PK__Lugar__35F8CED0EFED1D97");

            entity.ToTable("Lugar");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Latitud).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitud).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SitioWeb)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(15)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCiudadNavigation).WithMany(p => p.Lugar)
                .HasForeignKey(d => d.IdCiudad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Lugar__IdCiudad__0E6E26BF");
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.HasKey(e => e.IdNotificacion).HasName("PK__Notifica__F6CA0A85301620B5");

            entity.ToTable("Notificacion");

            entity.Property(e => e.EnlaceDestino)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FechaEnvio)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Leida).HasDefaultValue(false);
            entity.Property(e => e.Mensaje).HasColumnType("text");
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Titulo)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Notificacions)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificac__IdUsu__0F624AF8");
        });

        modelBuilder.Entity<PoliticaLegal>(entity =>
        {
            entity.HasKey(e => e.IdPolitica).HasName("PK__Politica__B7DF5F46BD6B1BDA");

            entity.ToTable("PoliticaLegal");

            entity.HasIndex(e => e.Tipo, "UQ__Politica__8E762CB46B313B15").IsUnique();

            entity.Property(e => e.Contenido).HasColumnType("text");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Tipo)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Version)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("1.0");
        });

        modelBuilder.Entity<Promocion>(entity =>
        {
            entity.HasKey(e => e.IdPromocion).HasName("PK__Promocio__35F6C2A5ADE0A266");

            entity.ToTable("Promocion");

            entity.HasIndex(e => e.Codigo, "UQ__Promocio__06370DAC25222236").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.FechaFin).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio).HasColumnType("datetime");
            entity.Property(e => e.TipoDescuento)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.UsoActual).HasDefaultValue(0);
            entity.Property(e => e.UsoMaximo).HasDefaultValue(1);
            entity.Property(e => e.ValorDescuento).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.Promocions)
                .HasForeignKey(d => d.IdEvento)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Promocion__IdEve__10566F31");

            entity.HasOne(d => d.IdTipoEntradaNavigation).WithMany(p => p.Promocions)
                .HasForeignKey(d => d.IdTipoEntrada)
                .HasConstraintName("FK__Promocion__IdTip__114A936A");
        });

        modelBuilder.Entity<Sesion>(entity =>
        {
            entity.HasKey(e => e.IdSesion).HasName("PK__Sesion__22EC535B2A62A112");

            entity.ToTable("Sesion");

            entity.Property(e => e.Activa).HasDefaultValue(true);
            entity.Property(e => e.CerradaPorUsuario).HasDefaultValue(false);
            entity.Property(e => e.Dispositivo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaExpiracion).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NroIp)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("NroIp");
            entity.Property(e => e.Token)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Sesions)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Sesion__IdUsuari__123EB7A3");
        });

        modelBuilder.Entity<TipoEntrada>(entity =>
        {
            entity.HasKey(e => e.IdTipoEntrada).HasName("PK__TipoEntr__00A2A1DE568ABCF4");

            entity.HasIndex(e => new { e.IdEvento, e.Nombre }, "UQ__TipoEntr__F410C2F958B4EAE5").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaFinVenta).HasColumnType("datetime");
            entity.Property(e => e.FechaInicioVenta).HasColumnType("datetime");
            entity.Property(e => e.MaximoCompra).HasDefaultValue(10);
            entity.Property(e => e.MinimoCompra).HasDefaultValue(1);
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.TipoEntrada)
                .HasForeignKey(d => d.IdEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TipoEntra__IdEve__1332DBDC");
        });

        modelBuilder.Entity<TipoUsuario>(entity =>
        {
            entity.HasKey(e => e.IdTipoUsuario).HasName("PK__TipoUsua__CA04062B346013DF");

            entity.ToTable("TipoUsuario");

            entity.HasIndex(e => e.Nombre, "UQ__TipoUsua__75E3EFCFF42F4DDE").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__5B65BF97CB484F60");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.CorreoElectronico, "UQ__Usuario__531402F32E3534A5").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Apellidos)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AutenticacionDosPasos).HasDefaultValue(false);
            entity.Property(e => e.Bloqueado).HasDefaultValue(false);
            entity.Property(e => e.Ciudad)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ContrasenaHash)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.CorreoElectronico)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Genero)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.IntentosFallidos).HasDefaultValue(0);
            entity.Property(e => e.MetodoVerificacionPreferido)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NotificarInicioSesion).HasDefaultValue(true);
            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Pais)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RestablecerExpira).HasColumnType("datetime");
            entity.Property(e => e.RestablecerToken)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TipoDocumento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UltimoIntentoFallido).HasColumnType("datetime");
            entity.Property(e => e.VerificadoEmail).HasDefaultValue(false);
        });

        modelBuilder.Entity<UsuarioRol>(entity =>
        {
            entity.HasKey(e => e.IdUsuarioRol).HasName("PK__UsuarioR__6806BF4AEBC6F6C8");

            entity.ToTable("UsuarioRol");

            entity.HasIndex(e => new { e.IdUsuario, e.IdTipoUsuario }, "UQ__UsuarioR__F7C5FFF47E316BF1").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaAsignacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdTipoUsuarioNavigation).WithMany(p => p.UsuarioRols)
                .HasForeignKey(d => d.IdTipoUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UsuarioRo__IdTip__14270015");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.UsuarioRols)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UsuarioRo__IdUsu__151B244E");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
