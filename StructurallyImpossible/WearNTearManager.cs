namespace StructurallyImpossible
{
    public static class WearNTearManager
    {

        public const int HasFieldsHash = -310439593; // HasFields
        public const int HasFieldsWearNTearHash = 2128458598; // HasFieldsWearNTear
        public const int WearNTearNoSupportWearHash = -1943636578; // WearNTear.m_noSupportWear

        public static void SetNoSupportWear(WearNTear wearNTear)
        {
            wearNTear.m_noSupportWear = false;
            wearNTear.m_nview.m_zdo.Set(HasFieldsHash, true);
            wearNTear.m_nview.m_zdo.Set(HasFieldsWearNTearHash, true);
            wearNTear.m_nview.m_zdo.Set(WearNTearNoSupportWearHash, false);
        }
    }
}
