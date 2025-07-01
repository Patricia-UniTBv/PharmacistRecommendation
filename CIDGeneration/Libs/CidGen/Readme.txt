Am creat o versiune actualizată a dll-ului de generare CID din CNP, compilată pentru a putea fi apelata din Excel, sau orice aplicaţie capabilă de interacţiune prin OLE/COM, împreuna cu un fişier Excel care conţine un exemplu de cod.

Inainte de utilizare, dezarhivaţi fiţierele într-o locaţie accesibilă.
Lansaţi cmd.exe cu drepturi de admin, schimbaţi calea curentă către cea de mai sus.
Apoi rulati comanda de mai jos: (! Trebuie sa aveti .NET Framework 2.0 instalat !)

> C:\Windows\Microsoft.NET\Framework\v2.0.50727\regasm.exe Cnas.Siui.CidGen.dll /tlb /codebase

Exista in arhiva doua scurtaturi pentru inregistrarea si dezinregistrarea bibliotecii.

Deschideti apoi excelul si verificati ca aveti in referinte "CNAS-SIUI - Generator CID".
