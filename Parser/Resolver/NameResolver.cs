using Common;
using Parser.ParseTree;
using System.Collections.Generic;
using System.Linq;

namespace Parser.Resolver
{
    internal static class NameResolver
    {
        public static void Resolve(ParserContext parser, CompilationScope scope)
        {
            Dictionary<string, TopLevelConstruct> scopeLookup = new Dictionary<string, TopLevelConstruct>();
            Dictionary<string, TopLevelConstruct> depsLookup = new Dictionary<string, TopLevelConstruct>();
            Dictionary<string, NamespaceReferenceTemplate> depsNamespaceLookup = new Dictionary<string, NamespaceReferenceTemplate>();

            // First create a lookup of JUST the libraries that are available to this library.
            // This is localized to the locales that are used by that library.
            foreach (LocalizedLibraryView depLocView in scope.Dependencies)
            {
                depLocView.LibraryScope.FlattenFullyQualifiedLookupsIntoGlobalLookup(depsLookup, depLocView.Locale);
                Util.MergeDictionaryInto(
                    depLocView.LibraryScope.GetFlattenedNamespaceLookup(depLocView.Locale),
                    depsNamespaceLookup);
            }

            scope.FlattenFullyQualifiedLookupsIntoGlobalLookup(scopeLookup, scope.Locale);
            Dictionary<string, NamespaceReferenceTemplate> scopeNamespaceLookup = scope.GetFlattenedNamespaceLookup(scope.Locale);
            TopLevelConstruct[] toResolve = scope.GetTopLevelConstructs();

            ResolveNames(parser, toResolve, scopeLookup, depsLookup, scopeNamespaceLookup, depsNamespaceLookup);
        }

        // Note that the lookup will also contain the toResolve entries plus the entities from the dependencies.
        public static void ResolveNames(
            ParserContext parser,
            TopLevelConstruct[] toResolve,
            Dictionary<string, TopLevelConstruct> scopeLookup,
            Dictionary<string, TopLevelConstruct> depsLookup,
            Dictionary<string, NamespaceReferenceTemplate> scopeNamespaceLookup,
            Dictionary<string, NamespaceReferenceTemplate> depsNamespaceLookup)
        {
            using (new PerformanceSection("ResolveNames"))
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

                foreach (TopLevelConstruct item in toResolve)
                {
                    if (!(item is Namespace))
                    {
                        item.ResolveNames(parser);
                    }
                }

                foreach (ClassDefinition cd in classes)
                {
                    cd.ResolveMemberIds();
                }

                foreach (TopLevelConstruct ex in toResolve.Where(ex => ex is ConstStatement || ex is EnumDefinition))
                {
                    parser.ConstantAndEnumResolutionState[ex] = ConstantResolutionState.NOT_RESOLVED;
                }
            }
        }
    }
}
