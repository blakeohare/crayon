using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser.Resolver
{
    internal static class EntityNameResolver
    {
        public static void Resolve(ParserContext parser, CompilationScope scope)
        {
            Dictionary<string, TopLevelEntity> scopeLookup = new Dictionary<string, TopLevelEntity>();
            Dictionary<string, TopLevelEntity> depsLookup = new Dictionary<string, TopLevelEntity>();
            Dictionary<string, NamespaceReferenceTemplate> depsNamespaceLookup = new Dictionary<string, NamespaceReferenceTemplate>();

            // First create a lookup of JUST the libraries that are available to this library.
            // This is localized to the locales that are used by that library.
            foreach (LocalizedAssemblyView depLocView in scope.Dependencies)
            {
                depLocView.Scope.FlattenFullyQualifiedLookupsIntoGlobalLookup(depsLookup, depLocView.Locale);
                Dictionary<string, NamespaceReferenceTemplate> newNamespaces = depLocView.Scope.GetFlattenedNamespaceLookup(depLocView.Locale);
                foreach (string key in newNamespaces.Keys)
                {
                    depsNamespaceLookup[key] = newNamespaces[key];
                }
            }

            scope.FlattenFullyQualifiedLookupsIntoGlobalLookup(scopeLookup, scope.Locale);
            Dictionary<string, NamespaceReferenceTemplate> scopeNamespaceLookup = scope.GetFlattenedNamespaceLookup(scope.Locale);
            TopLevelEntity[] toResolve = scope.GetTopLevelEntities();

            ResolveNames(parser, toResolve, scopeLookup, depsLookup, scopeNamespaceLookup, depsNamespaceLookup);
        }

        // Note that the lookup will also contain the toResolve entries plus the entities from the dependencies.
        public static void ResolveNames(
            ParserContext parser,
            TopLevelEntity[] toResolve,
            Dictionary<string, TopLevelEntity> scopeLookup,
            Dictionary<string, TopLevelEntity> depsLookup,
            Dictionary<string, NamespaceReferenceTemplate> scopeNamespaceLookup,
            Dictionary<string, NamespaceReferenceTemplate> depsNamespaceLookup)
        {
            foreach (FileScope file in new HashSet<FileScope>(toResolve.Select(tlc => tlc.FileScope)))
            {
                file.FileScopeEntityLookup.InitializeLookups(
                    depsLookup,
                    scopeLookup,
                    depsNamespaceLookup,
                    scopeNamespaceLookup);
            }

            List<ClassDefinition> classes = new List<ClassDefinition>(toResolve.OfType<ClassDefinition>());

            foreach (ClassDefinition cd in classes)
            {
                if (cd.BaseClassDeclarations.Length > 0)
                {
                    cd.ResolveBaseClasses();
                }
            }

            foreach (ClassDefinition cd in classes)
            {
                cd.VerifyNoBaseClassLoops();
            }

            foreach (TopLevelEntity item in toResolve)
            {
                if (!(item is Namespace))
                {
                    item.ResolveEntityNames(parser);
                }
            }

            foreach (ClassDefinition cd in classes)
            {
                cd.ResolveMemberIds();
            }

            foreach (TopLevelEntity ex in toResolve.Where(ex => ex is ConstDefinition || ex is EnumDefinition))
            {
                parser.ConstantAndEnumResolutionState[ex] = ConstantResolutionState.NOT_RESOLVED;
            }
        }
    }
}
