using System;
using System.Linq;
using System.Text;

namespace Meziantou.Framework.CodeDom
{
    public class TypeReference : Expression
    {
        private TypeDeclaration _typeDeclaration;
        private string _name;
        private string _namespace;
        private CodeObjectCollection<TypeReference> _parameters;
        private CodeObjectCollection<TypeReference> _typeDeclarationParameters;

        public TypeReference()
        {
        }

        public TypeReference(TypeDeclaration typeDeclaration)
        {
            _typeDeclaration = typeDeclaration ?? throw new ArgumentNullException(nameof(typeDeclaration));
        }

        public TypeReference(string clrFullTypeName)
        {
            var parsedType = ParsedType.Parse(clrFullTypeName);
            FromParsedType(parsedType);
        }

        private void FromParsedType(ParsedType parsedType)
        {
            Namespace = parsedType.Namespace;
            Name = parsedType.Name;
            if (parsedType.Arguments != null)
            {
                foreach (var argument in parsedType.Arguments)
                {
                    var typeReference = new TypeReference();
                    typeReference.FromParsedType(argument);
                    Parameters.Add(typeReference);
                }
            }
        }

        public TypeReference(Type type) : this(type.FullName)
        {
        }

        public string Name
        {
            get
            {
                if (_typeDeclaration != null)
                    return _typeDeclaration.Name;

                return _name;
            }
            set
            {
                _name = value;
                _typeDeclaration = null;
            }
        }

        public string Namespace
        {
            get
            {
                if (_typeDeclaration != null)
                    return _typeDeclaration.Namespace;

                return _namespace;
            }
            set
            {
                _namespace = value;
                _typeDeclaration = null;
            }
        }

        public CodeObjectCollection<TypeReference> Parameters
        {
            get
            {
                if (_typeDeclaration is IParametrableType typeParameter)
                {
                    if (_typeDeclarationParameters == null)
                    {
                        var collection = new CodeObjectCollection<TypeReference>(this);
                        collection.AddRange(typeParameter.Parameters.Select(p => new TypeReference(p.Name)));
                        _typeDeclarationParameters = collection;
                    }

                    return _typeDeclarationParameters;
                }

                if (_parameters == null)
                {
                    _parameters = new CodeObjectCollection<TypeReference>(this);
                }

                return _parameters;
            }
        }

        public string ClrFullTypeName
        {
            get
            {
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(Namespace))
                {
                    sb.Append(Namespace).Append(".");
                }

                sb.Append(Name);
                if (Parameters.Any())
                {
                    sb.Append('<');
                    var first = true;
                    foreach (var parameter in Parameters)
                    {
                        if (!first)
                        {
                            sb.Append(", ");
                        }

                        sb.Append(parameter.ClrFullTypeName);

                        first = false;
                    }
                    sb.Append('>');
                }

                return sb.ToString();
            }
        }

        public TypeReference Clone()
        {
            var clone = new TypeReference();
            clone._name = _name;
            clone._namespace = _namespace;
            clone._typeDeclaration = _typeDeclaration;
            if (_parameters != null)
            {
                clone._parameters = new CodeObjectCollection<TypeReference>(clone);
                foreach (var parameter in _parameters)
                {
                    clone._parameters.Add(parameter);
                }
            }
            return clone;
        }

        public static implicit operator TypeReference(TypeDeclaration typeDeclaration)
        {
            return new TypeReference(typeDeclaration);
        }

        public static implicit operator TypeReference(Type type)
        {
            return new TypeReference(type);
        }
    }
}