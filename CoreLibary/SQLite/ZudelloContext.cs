using System;
using System.Dynamic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ZudelloApi;
//using ZudelloThinClient.Attache.AttacheSettings;

namespace ZudelloThinClientLibary
{


    public partial class ZudelloContext : DbContext      


    {

        private readonly string _connection;
        public ZudelloContext()  
        {
            string json = File.ReadAllText(@"ZudelloConnections\ConnectionDetails.json");
            dynamic myObj = JsonConvert.DeserializeObject<ExpandoObject>(json);
         //   Console.WriteLine(json);
          //  Console.ReadLine();
            _connection = myObj.ConnectionStrings.Storage;
        }

        public ZudelloContext(DbContextOptions<ZudelloContext> options)
            : base(options)
        {
        }
        //Issue with this using the builder. When compiling into .MSI file.


        public virtual DbSet<Zconnections> Zconnections { get; set; }
        public virtual DbSet<Zhashlog> Zhashlog { get; set; }
        public virtual DbSet<Zmapping> Zmapping { get; set; }
        public virtual DbSet<Zqueue> Zqueue { get; set; }
        public virtual DbSet<Zsettings> Zsettings { get; set; }
        public virtual DbSet<Zlastsync> Zlastsync { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            

            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                try 
                { 

                    optionsBuilder.UseSqlite(_connection);
                }

                catch(Exception ex)
                { 
                
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Zconnections>(entity =>
            {
                entity.ToTable("ZCONNECTIONS");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ConnectionUuid)
                    .IsRequired()
                    .HasColumnName("connection_uuid");

                entity.Property(e => e.DataSource)
                    .IsRequired()
                    .HasColumnName("dataSource");

                entity.Property(e => e.InitialCatalog).IsRequired();

                entity.Property(e => e.IntergrationType)
                    .IsRequired()
                    .HasColumnName("intergrationType");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasColumnName("userId");

                entity.Property(e => e.ZudelloCredentials).HasColumnName("zudelloCredentials");

                entity.Property(e => e.UseIS).HasColumnName("useIS");
            });


            modelBuilder.Entity<Zhashlog>(entity =>
            {
                entity.ToTable("ZHASHLOG");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("Created_at")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("current_timestamp");

                entity.Property(e => e.Hash)
                    .HasColumnName("hash")
                    .HasColumnType("byte(16)");

                entity.Property(e => e.MappingId)
                    .HasColumnName("Mapping_id")
                    .HasColumnType("INT");

                entity.HasOne(d => d.Mapping)
                    .WithMany(p => p.Zhashlog)
                    .HasForeignKey(d => d.MappingId);
            });



            modelBuilder.Entity<Zlastsync>(entity =>
            {
                entity.ToTable("ZLASTSYNC");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.LastSync)
                    .HasColumnName("lastSync")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("current_timestamp");

                entity.Property(e => e.MappingId)
                    .HasColumnName("Mapping_id")
                    .HasColumnType("INT");

                entity.Property(e => e.lastID)
                   .HasColumnName("lastID")
                   .ValueGeneratedNever();

                entity.HasOne(d => d.Mapping)
                    .WithMany(p => p.Zlastsync)
                    .HasForeignKey(d => d.MappingId);
            });


            modelBuilder.Entity<Zmapping>(entity =>
            {
                entity.ToTable("ZMAPPING");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.connection_id).HasColumnName("connection_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("Created_at")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("current_timestamp");

                entity.Property(e => e.DeletedAt)
                    .HasColumnName("Deleted_at")
                    .HasColumnType("datetime");

             
                entity.Property(e => e.IntergrationUuid).HasColumnName("intergration_uuid");

                entity.Property(e => e.IsMasterData).HasColumnType("TINYINT");

                entity.Property(e => e.IsOutgoing).HasColumnType("TINYINT");

                entity.Property(e => e.ProcessOrder).HasColumnType("int");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("Updated_at")
                    .HasColumnType("Timestamp DATETIME")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.Connection)
                    .WithMany(p => p.Zmapping)
                    .HasForeignKey(d => d.connection_id);
            });

            modelBuilder.Entity<Zqueue>(entity =>
            {
                entity.ToTable("ZQUEUE");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.ConnectionId).HasColumnName("connection_id");
                //entity.Property(e => e.FailedCount);
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("Created_at")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("current_timestamp");

                entity.Property(e => e.Queue_Id).HasColumnName("queue_id")
                    .HasColumnType("int");

                entity.Property(e => e.MappingId)
                    .HasColumnName("Mapping_id")
                    .HasColumnType("int");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("Updated_at")
                    .HasColumnType("Timestamp DATETIME")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.ResponseSent).HasColumnName("responseSent")
                     .HasColumnType("int");

                entity.HasOne(d => d.Connection)
                    .WithMany(p => p.Zqueue)
                    .HasForeignKey(d => d.ConnectionId);

                entity.Property(e => e.Exception);
                entity.HasOne(d => d.Mapping)
                    .WithMany(p => p.Zqueue)
                    .HasForeignKey(d => d.MappingId);
            });

            modelBuilder.Entity<Zsettings>(entity =>
            {
                entity.ToTable("ZSETTINGS");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("Created_at")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("current_timestamp");

                entity.Property(e => e.DeletedAt)
                    .HasColumnName("Deleted_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Key).IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("Updated_at")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("current_timestamp");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
