using System;
using System.Collections.Generic;
using Funhouse.Models.Projections;
using Funhouse.Projections;

namespace Funhouse.Services
{
    public interface IProjectionRegistry
    {
        void Register(ProjectionType type, IProjection projection);
        IProjection GetProjection(ProjectionType type);
    }

    public class ProjectionRegistry : IProjectionRegistry
    {
        public ProjectionRegistry()
        {
            Projections = new Dictionary<ProjectionType, IProjection>();
        }

        private IDictionary<ProjectionType, IProjection> Projections { get; }

        public void Register(ProjectionType type, IProjection projection)
        {
            Projections[type] = projection;
        }

        public IProjection GetProjection(ProjectionType type)
        {
            if (!Projections.TryGetValue(type, out var projection)) throw new InvalidOperationException($"Projection {type} not registered");

            return projection;
        }
    }
}