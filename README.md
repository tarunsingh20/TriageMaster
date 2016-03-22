# Triage Master Tool Overview
Triage Master is a tool to facilitate in Defect/Bug Triage process for projects that use Microsoft VSTS (TFS 2012/2013/2015) for Application Lifecycle Management. 

Triage Master provides the following functionalities:

1. Ability to Show Aging of defects/bugs and color code based on thresholds specified in setting. 
2. Ability to edit multiple defects/bugs at same time, going beyond what can be done through Excel.
3. Ability to link one or more defects/bugs to a work item.
4. Ability to find related defect/bugs using smart string matching algorithms and fuzzy logic. 
5. Ability to export the defect/bug report to excel.

Refer to User Guide available in the release section for more information on how to install and use the tool

# Asset BOM

The application is installed using an MSI file. The files bundled within the installer are :

1. TriageMaster.exe 
2. TriageMaster.exe.config
3. TriageMaster Library DLL’s
4. Dependent dll’s such as Xceed.Wpf.Toolkit ( for progress bar) and nuget package dll’s.

Note: The Triage Master is based on two external open source components : 

1. Xceed Wpf Toolkit ( for Progress Bar/Busy Indicator) https://wpftoolkit.codeplex.com
2. Fuzzy String Matching Algorithms : http://fuzzystring.codeplex.com
3. Redistributable TFS API dll’s from nuget https://www.visualstudio.com/integrate/get-started/client-libraries/dotnet

# Licensing
The underlying source code for this tool is licensed under [MIT License](http://opensource.org/licenses/mit-license.php)
The additional open source components listed above are governed by their respective licenses.

#Contributors
@tarunsingh20, @zubair-am , @sudhakarreddy-pr, @rajendrosahu
