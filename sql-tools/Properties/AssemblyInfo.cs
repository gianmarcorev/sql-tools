using CommandLine;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Le informazioni generali relative a un assembly sono controllate dal seguente 
// set di attributi. Modificare i valori di questi attributi per modificare le informazioni
// associate a un assembly.
[assembly: AssemblyTitle("sql-tools")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("sql-tools")]
[assembly: AssemblyCopyright("Copyright © Gianmarco Reverberi 2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Se si imposta ComVisible su false, i tipi in questo assembly non saranno visibili 
// ai componenti COM. Se è necessario accedere a un tipo in questo assembly da 
// COM, impostare su true l'attributo ComVisible per tale tipo.
[assembly: ComVisible(false)]

// Se il progetto viene esposto a COM, il GUID seguente verrà utilizzato come ID della libreria dei tipi
[assembly: Guid("d0ae9db8-5577-4534-9fb8-ce34fac553da")]

// Le informazioni sulla versione di un assembly sono costituite dai seguenti quattro valori:
//
//      Versione principale
//      Versione secondaria 
//      Numero di build
//      Revisione
//
// È possibile specificare tutti i valori oppure impostare valori predefiniti per i numeri relativi alla revisione e alla build 
// usando l'asterisco '*' come illustrato di seguito:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0")]

[assembly: AssemblyLicense(
    "This is free software. You may redistribute copies of it under the terms of",
    "the MIT License <http://www.opensource.org/licenses/mit-license.php>.")]
[assembly: AssemblyUsage(
    "Usage: YourApp -rMyData.in -wMyData.out --calculate",
    "       YourApp -rMyData.in -i -j9.7 file0.def file1.def")]