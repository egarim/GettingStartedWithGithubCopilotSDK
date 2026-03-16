// ...
var containsMarker = answer?.Data.Content?.Contains(SkillMarker) ?? false;
PrintProp("Contiene marcador:", containsMarker);
Console.WriteLine(!containsMarker
    ? "  OK Skill desactivado correctamente - marcador ausente!"
    : "  AVISO Marcador encontrado a pesar de estar desactivado.");