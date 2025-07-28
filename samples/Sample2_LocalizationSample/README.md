This sample shows how to create translations for you extension and how to 
use it in your extension.

Localization data is automatically loaded from the `Localization` folder in
your project, make sure to copy it to output directory with

```xml
    <ItemGroup>
        <Content Include="Localization\*">
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </Content>
    </ItemGroup>
```