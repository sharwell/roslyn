﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.VisualStudio.IntegrationTest.Utilities;
using Microsoft.VisualStudio.IntegrationTest.Utilities.Harness;
using Xunit;

namespace Roslyn.VisualStudio.IntegrationTests.CSharp
{
    [Collection(nameof(SharedIntegrationHostFixture))]
    public class CSharpAddMissingReference : AbstractIdeEditorTest
    {
        private const string FileInLibraryProject1 = @"Public Class Class1
    Inherits System.Windows.Forms.Form
    Public Sub goo()

    End Sub
End Class

Public Class class2
    Public Sub goo(ByVal x As System.Windows.Forms.Form)

    End Sub

    Public Event ee As System.Windows.Forms.ColumnClickEventHandler
End Class

Public Class class3
    Implements System.Windows.Forms.IButtonControl

    Public Property DialogResult() As System.Windows.Forms.DialogResult Implements System.Windows.Forms.IButtonControl.DialogResult
        Get

        End Get
        Set(ByVal Value As System.Windows.Forms.DialogResult)

        End Set
    End Property

    Public Sub NotifyDefault(ByVal value As Boolean) Implements System.Windows.Forms.IButtonControl.NotifyDefault

    End Sub

    Public Sub PerformClick() Implements System.Windows.Forms.IButtonControl.PerformClick

    End Sub
End Class
";
        private const string FileInLibraryProject2 = @"Public Class Class1
    Inherits System.Xml.XmlAttribute
    Sub New()
        MyBase.New(Nothing, Nothing, Nothing, Nothing)
    End Sub
    Sub goo()

    End Sub
    Public bar As ClassLibrary3.Class1
End Class
";
        private const string FileInLibraryProject3 = @"Public Class Class1
    Public Enum E
        E1
        E2
    End Enum

    Public Function Goo() As ADODB.Recordset
        Dim x As ADODB.Recordset = Nothing
        Return x
    End Function


End Class
";
        private const string FileInConsoleProject1 = @"
class Program
{
    static void Main(string[] args)
    {
        var y = new ClassLibrary1.class2();
        y.goo(null);

        y.ee += (_, __) => { };

        var x = new ClassLibrary1.Class1();

        ClassLibrary1.class3 z = null;

        var a = new ClassLibrary2.Class1();
        var d = a.bar;
    }
}
";

        private const string ClassLibrary1Name = "ClassLibrary1";
        private const string ClassLibrary2Name = "ClassLibrary2";
        private const string ClassLibrary3Name = "ClassLibrary3";
        private const string ConsoleProjectName = "ConsoleApplication1";

        protected override string LanguageName => LanguageNames.CSharp;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync().ConfigureAwait(true);
            await VisualStudio.SolutionExplorer.CreateSolutionAsync("ReferenceErrors", solutionElement: XElement.Parse(
                "<Solution>" +
               $"   <Project ProjectName=\"{ClassLibrary1Name}\" ProjectTemplate=\"{WellKnownProjectTemplates.WinFormsApplication}\" Language=\"{LanguageNames.VisualBasic}\">" +
                "       <Document FileName=\"Class1.vb\"><![CDATA[" +
                FileInLibraryProject1 +
                "]]>" +
                "       </Document>" +
                "   </Project>" +
               $"   <Project ProjectName=\"{ClassLibrary2Name}\" ProjectReferences=\"{ClassLibrary3Name}\" ProjectTemplate=\"{WellKnownProjectTemplates.ClassLibrary}\" Language=\"{LanguageNames.VisualBasic}\">" +
                "       <Document FileName=\"Class1.vb\"><![CDATA[" +
               FileInLibraryProject2 +
                "]]>" +
                "       </Document>" +
                "   </Project>" +
               $"   <Project ProjectName=\"{ClassLibrary3Name}\" ProjectTemplate=\"{WellKnownProjectTemplates.ClassLibrary}\" Language=\"{LanguageNames.VisualBasic}\">" +
                "       <Document FileName=\"Class1.vb\"><![CDATA[" +
               FileInLibraryProject3 +
                "]]>" +
                "       </Document>" +
                "   </Project>" +
               $"   <Project ProjectName=\"{ConsoleProjectName}\" ProjectReferences=\"{ClassLibrary1Name};{ClassLibrary2Name}\" ProjectTemplate=\"{WellKnownProjectTemplates.ConsoleApplication}\" Language=\"{LanguageNames.CSharp}\">" +
                "       <Document FileName=\"Program.cs\"><![CDATA[" +
               FileInConsoleProject1 +
                "]]>" +
                "       </Document>" +
               "   </Project>" +
                "</Solution>"));
        }

        [IdeFact, Trait(Traits.Feature, Traits.Features.AddMissingReference)]
        public async Task VerifyAvailableCodeActionsAsync()
        {
            await VisualStudio.SolutionExplorer.OpenFileAsync(ConsoleProjectName, "Program.cs");
            await VisualStudio.Editor.PlaceCaretAsync("y.goo", charsOffset: 1);
            await VisualStudio.Editor.InvokeCodeActionListAsync();
            await VisualStudio.Editor.Verify.CodeActionAsync("Add reference to 'System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'.", applyFix: false);
            await VisualStudio.Editor.PlaceCaretAsync("y.ee", charsOffset: 1);
            await VisualStudio.Editor.InvokeCodeActionListAsync();
            await VisualStudio.Editor.Verify.CodeActionAsync("Add reference to 'System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'.", applyFix: false);
            await VisualStudio.Editor.PlaceCaretAsync("a.bar", charsOffset: 1);
            await VisualStudio.Editor.InvokeCodeActionListAsync();
            await VisualStudio.Editor.Verify.CodeActionAsync("Add project reference to 'ClassLibrary3'.", applyFix: false);
        }

        [IdeFact, Trait(Traits.Feature, Traits.Features.AddMissingReference)]
        public async Task InvokeSomeFixesInCSharpThenVerifyReferencesAsync()
        {
            await VisualStudio.SolutionExplorer.OpenFileAsync(ConsoleProjectName, "Program.cs");
            await VisualStudio.Editor.PlaceCaretAsync("y.goo", charsOffset: 1);
            await VisualStudio.Editor.InvokeCodeActionListAsync();
            await VisualStudio.Editor.Verify.CodeActionAsync("Add reference to 'System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'.", applyFix: true);
            VisualStudio.SolutionExplorer.Verify.AssemblyReferencePresent(
                projectName: ConsoleProjectName,
                assemblyName: "System.Windows.Forms",
                assemblyVersion: "4.0.0.0",
                assemblyPublicKeyToken: "b77a5c561934e089");
            await VisualStudio.Editor.PlaceCaretAsync("a.bar", charsOffset: 1);
            await VisualStudio.Editor.InvokeCodeActionListAsync();
            await VisualStudio.Editor.Verify.CodeActionAsync("Add project reference to 'ClassLibrary3'.", applyFix: true);
            VisualStudio.SolutionExplorer.Verify.ProjectReferencePresent(
                projectName: ConsoleProjectName,
                referencedProjectName: ClassLibrary3Name);
        }
    }
}
