// ...
var containsMarker = answer?.Data.Content?.Contains(SkillMarker) ?? false;
PrintProp("Contiene marcador:", containsMarker);
Console.WriteLine(containsMarker
    ? "  OK Skill cargado y aplicado exitosamente!"
    : "  AVISO Marcador no encontrado - skill puede no haberse aplicado.");