using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaIgreja.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCategoriasPatrimonioIniciais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CategoriasPatrimonio",
                columns: new[] { "Id", "Ativo", "DataCriacao", "Descricao", "Nome" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Moveis em geral", "Moveis" },
                    { 2, true, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cadeiras, mesas e similares", "Cadeiras e mesas" },
                    { 3, true, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Instrumentos e acessorios musicais", "Instrumentos musicais" },
                    { 4, true, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Caixas, mesas, microfones e audio", "Equipamentos de audio" },
                    { 5, true, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Projetores, TVs, cameras e video", "Equipamentos de video" },
                    { 6, true, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Refletores, spots e iluminacao", "Iluminacao" },
                    { 7, true, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Notebooks, computadores e perifericos", "Informatica" },
                    { 8, true, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Geladeiras, micro-ondas e afins", "Eletrodomesticos" },
                    { 9, true, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Carros, vans e motos", "Veiculos" },
                    { 10, true, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Brinquedos, mobiliario e itens infantis", "Material infantil" },
                    { 11, true, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Aspiradores, enceradeiras e afins", "Equipamentos de limpeza" },
                    { 12, true, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Itens de apoio e uso geral", "Utensilios gerais" },
                    { 13, true, new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bens de escritorio e administracao", "Patrimonio administrativo" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "CategoriasPatrimonio",
                keyColumn: "Id",
                keyValue: 13);
        }
    }
}
