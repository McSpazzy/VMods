namespace LetThemCook
{
    public static class CookingStationManager
    {
        public const int HasFieldsHash = -310439593; // HasFields
        public const int HasFieldsCookingStationHash = -455618563; // HasFieldsCookingStation
        public const int CookingStationCanOvercookItemsHash = 628621776; // CookingStation.m_canOvercookItems

        public static void SetCanOvercookItems(CookingStation cookingStation)
        {
            cookingStation.m_canOvercookItems = false;
            cookingStation.m_nview.m_zdo.Set(HasFieldsHash, true);
            cookingStation.m_nview.m_zdo.Set(HasFieldsCookingStationHash, true);
            cookingStation.m_nview.m_zdo.Set(CookingStationCanOvercookItemsHash, false);
        }
    }
}
