﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Train_D.Models;

namespace Train_D.Data.Config
{
    public class StationConfig : IEntityTypeConfiguration<Station>
    {
        public void Configure(EntityTypeBuilder<Station> builder)
        {
            builder.HasKey(s => s.StationName);

            builder.Property(l => l.Latitude)
                .HasColumnType("DECIMAL(12,9)");
            builder.Property(l => l.Longitude)
                .HasColumnType("DECIMAL(12,9)");


            builder
                .HasMany(t => t.TripsStart)
                .WithOne(s => s.StationBegain)
                .HasForeignKey(t => t.StartStation)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(t => t.TripsEnd)
                .WithOne(s => s.StationEnd)
                .HasForeignKey(t => t.EndStaion)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
