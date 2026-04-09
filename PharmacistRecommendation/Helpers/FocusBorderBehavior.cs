namespace PharmacistRecommendation.Helpers
{
    /// <summary>
    /// Attach to any Entry that sits directly inside a MAUI Border.
    /// On focus the Border stroke changes to PrimaryLight; on unfocus it reverts to BorderDefault.
    /// </summary>
    public class FocusBorderBehavior : Behavior<Entry>
    {
        private static readonly Color FocusedColor  = Color.FromArgb("#1A3A5C"); // PrimaryLight
        private static readonly Color DefaultColor  = Color.FromArgb("#C9D6EA"); // BorderDefault

        protected override void OnAttachedTo(Entry entry)
        {
            base.OnAttachedTo(entry);
            entry.Focused   += OnFocused;
            entry.Unfocused += OnUnfocused;
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            base.OnDetachingFrom(entry);
            entry.Focused   -= OnFocused;
            entry.Unfocused -= OnUnfocused;
        }

        private static void OnFocused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry && entry.Parent is Border border)
                border.Stroke = new SolidColorBrush(FocusedColor);
        }

        private static void OnUnfocused(object sender, FocusEventArgs e)
        {
            if (sender is Entry entry && entry.Parent is Border border)
                border.Stroke = new SolidColorBrush(DefaultColor);
        }
    }
}
