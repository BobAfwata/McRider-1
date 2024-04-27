using System.Reflection;

namespace McRider.Common.Helpers;

public static class AssemblyHelpers
{
    public static IEnumerable<string> GetAppResources()
    {
        List<string> ret = new List<string>();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assem in assemblies)
        {
            try
            {
                string[] resourceNames = assem.GetManifestResourceNames();
                foreach (string name in resourceNames)
                    ret.Add(name);
            }
            catch (Exception ex)
            {
                ret.Add($"Exception reading {assem.GetName()}. {ex.ToString()}");
            }
        }
        return ret;
    }

    public static Assembly GetAssemblyForResource(string path)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assem in assemblies)
        {
            try
            {
                string[] resourceNames = assem.GetManifestResourceNames();
                foreach (string name in resourceNames)
                    if (name.Equals(path))
                        return assem;
            }
            catch (Exception)
            {
                /*Intentionlly eatting, don't want to end the app if there's an issue reading a manifest*/
            }
        }

        return null;
    }
}
