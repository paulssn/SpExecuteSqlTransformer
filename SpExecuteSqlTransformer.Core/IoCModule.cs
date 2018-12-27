using Ninject.Modules;
using SpExecuteSqlTransformer.Core.Manipulators;

namespace SpExecuteSqlTransformer.Core
{
    public class IoCModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IManipulator>().To<StringUnwrapper>();
            Bind<IManipulator>().To<ParamReplacer>();
            Bind<IManipulator>().To<Formatter>();
            Bind<ITransformationManager>().To<TransformationManager>();
        }
    }
}
