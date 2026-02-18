using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FromParameterAttribute : Attribute
{
    public string ParameterName { get; }

    public FromParameterAttribute(string parameterName)
    {
        ParameterName = parameterName;
    }
}