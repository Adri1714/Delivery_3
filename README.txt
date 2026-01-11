Scripts for exporting/importing data:
- collector.php: PHP dynamic analytics event collector that stores game telemetry data in a MySQL database.
- db_connect.php: Database connection configuration file for MySQL.
- fetch_data.php: Data retrieval API endpoint that fetches analytics data from the database.
- setup.php: Schema migration tool that ensures the database structure matches the analytics events defined in the Unity game. It's typically run once or whenever new event types or parameters are added to the game, automatically creating or updating the necessary database tables to store the analytics data.

Scene with implementation:
- data_visualization for displaying data, heatmaps, path
- example_scene for playing and generating data

Heatmaps:
- The Heatmap shows the movement of the player along the map, the resolution and size of the points as the color of the texture are completely customizable, its necesary to add manually a material and the map because this helps to create the plane of the heatmap in the perfect position

Paths:
- Displayable character movement paths in the editor window that import the position data from the server, creates waypoints in the level and interpolates between them to create a path. These paths can be tweaked and are customizable from the inspector.

Event markers:
- The death marker shows the points where the character has died, if the player has died more than one time in the exact same spot, the code generates an offset for showing that there is more than one death.
- The deamage marker shows the points where the character has recieved damage, if the player has recieved damage more than one time in the exact same spot, the code generates an offset for showing that there is more than one damage instance.

Interaction controller:
- The Interaction cotroller is a GameObject. That acts as the main configuration and control hub for interaction data visualization. It defines how player paths, gameplay events (such as deaths or damage), and data from both current and previous sessions are rendered, including color gradients, visibility filters, transparency, and visual intensity.
It also manages heatmap settings, such as resolution, blur radius, smoothing, height offset, and the data capture interval used during gameplay.
Any change to these parameters is automatically propagated to the visualization system, ensuring consistent and up-to-date visual feedback in the editor and at runtime.


