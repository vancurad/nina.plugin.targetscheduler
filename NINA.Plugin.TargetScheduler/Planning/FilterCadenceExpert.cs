using NINA.Plugin.TargetScheduler.Database.Schema;

namespace NINA.Plugin.TargetScheduler.Planning {

    public class FilterCadenceExpert {
        /* WIP - notes
         *
         * This is probably used by an over-arching exposure planner that handles auto-mode, moon avoidance, etc
         *
         * - Loads or lazy-creates the in-memory cadence item list (and attaches to Target)
         * -
         * - Probably not:
         *   - Handles persistence as needed (including update of 'next' pointer)
         *   - Handles clearing rows as needed
         *
- Rows created during planning if not present - lazy load
- The rows for a target are cleared when:
	- Any change to associated EPs
	- Any change to Project filter or dither frequency
	- Any change to override exposure order
	- If exposure planning goes to 'auto export' mode
	- Target deletion
	- Not copied when target is copied
         */

        public FilterCadenceExpert(Target target) {
        }
    }
}