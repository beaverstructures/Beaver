# BeaverCore


<!-- ABOUT THE PROJECT -->
## About The Project
Beaver is a tool that allows parametric structural analysis and design of timber structures according to the European code “Eurocode5 - Design of Timber Structures”.
Beaver was conceived at the Polytechnic School of the University of São Paulo - Brazil, and developed by 

* [João Tavares Pini](https://www.linkedin.com/in/joao-pini/)
* [Márcio Sartorelli Venâncio de Souza](https://www.linkedin.com/in/marcio-sartorelli/)
* [Renan Prandini](https://www.linkedin.com/in/renan-prandini/)
<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- GETTING STARTED -->
## :baby: Getting Started

``` C#
public void TestULSDesign()
        {
            Displacement disp = new Displacement(1, "QA");
            List<Displacement> disps = new List<Displacement>() { disp };
            Force force = new Force(28, 3, 2, 0.2, 1.5, 0.4, "QA");
            List<Force> forces = new List<Force>() { force };

           
            Material mat = new Material("Glulam c","Softwood", 24*1e6, 17*1e6,
                0.5, 21.5, 2.5, 3.5, 11000,
                9100, 300, 650, 540,365,0); // kN/cm^2, EN 338:2016
            CroSec crosec = new CroSec_Rect(15, 15,mat);
            Point3D point = new Point3D();
            TimberFramePoint element = new TimberFramePoint(point,forces, disps, crosec, 1, 205, 205, 205, 0.7);
            TimberFrameULSResult result = element.ULSUtilization();

            /// extract results 
            List<double[]> UtilsY = result.UtilsY;

            for (int i=0; i < UtilsY.Count; i++)
            {
                // Utils0 values for all load combinations 
                Console.WriteLine(UtilsY[i][0]);
            }
        }
```


### Prerequisites
You will need the following libraries and/or software installed before getting to the fun!
* [Rhino 7.21+](https://www.rhino3d.com/download/)
* [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)


<!-- WORKFLOW EXAMPLES -->
## Workflow Overview

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONTRIBUTING -->
## Contributing
### Roadmap
- [ ] Model
  - [X] Actions (Forces/Displacement) Model
  - [X] Combinations Model
  - [X] Section Model
  - [X] Connections Model
  - [ ] GlobalModel containing sections, connections and corresponding connectivities
  - [ ] Integration with section and connection forces
- [ ] CI
    - [ ] Unit tests in BeaverTests
    - [ ] Github actions for automatic tests run in each PR

<!-- LICENSE -->
## License
Distributed under the MIT License. See `LICENSE.txt` for more information.
<p align="right">(<a href="#readme-top">back to top</a>)</p>

