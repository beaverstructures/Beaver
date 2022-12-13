# 🦫 BeaverCore


<!-- ABOUT THE PROJECT -->
## 🧬 About The Project
Beaver is a tool that allows parametric structural analysis and design of timber structures according to the European code “Eurocode5 - Design of Timber Structures”.
Beaver was conceived at the Polytechnic School of the University of São Paulo - Brazil, and developed by 

* [João Tavares Pini](https://www.linkedin.com/in/joao-pini/)
* [Márcio Sartorelli Venâncio de Souza](https://www.linkedin.com/in/marcio-sartorelli/)
* [Renan Prandini](https://www.linkedin.com/in/renan-prandini/)

BeaverCore is the core library of Beaver project. It is a .NET Standard 2.0 library which contains all logic to define timber beams and connections and analyse them by automatically generating load combinations and design checks. The choice for .NET Standard is to make Beaver logic accessible to any application and platform in .NET

BeaverCore is enhanced by [BeaverGrasshopper](https://www.food4rhino.com/en/app/beaver), a Grasshopper plug-in which uses BeaverCore logic to provide a Grasshopper plugin to generate and visualize design checks with options to import from Karamba3d plug-in. 

The active development branch is `develop`, the stable version is `master` and the release branch is `release`. Please create all your PRs to `develop`.

<!-- GETTING STARTED -->
## :baby: Getting Started
To make a simple analysis with BeaverCore, you can analyse a single TimberFramePoint providing the frame material, cross section, caracteristic displacements and internal forces. You can always reference the corresponding Eurocode sections of the analysis by navigating through the code. Better documentation for that matter is in the to-do list.

``` C#
public void TestULSDesign()
        {
            //Set permanent load displacement
            Displacement dispP = new Displacement(0.8, "P");
            //Set acidental load displacement
            Displacement dispQA = new Displacement(0.5, "QA");
            //Set displacement list
            List<Displacement> disps = new List<Displacement>() { dispQA, dispP };
            //Set permanent load internal forces
            Force force = new Force(50, 5, 4, 2.2, 6.8, 1.8, "P"); //N,Vy,Vz,Mt,My,Mz
            //Set acidental load internal forces
            Force force = new Force(28, 3, 2, 0.2, 1.5, 0.4, "QA"); //N,Vy,Vz,Mt,My,Mz
            Set internal force list
            List<Force> forces = new List<Force>() { force };

                
            //Set material and cross-section
            Material mat = new Material("Glulam c","Softwood", 24*1e6, 17*1e6,
                0.5, 21.5, 2.5, 3.5, 11000,
                9100, 300, 650, 540,365,0); // kN/cm^2, EN 338:2016
            CroSec crosec = new CroSec_Rect(15, 15,mat);
             
            Point3D point = new Point3D();
            // Define a timber frame point with the corresponding displacements, forces and parameters.
            // The constructor deals with generating load combinations
            TimberFramePoint element = new TimberFramePoint(point,forces, disps, crosec, 1, 205, 205, 205, 0.7);
            
            //Retrieve the ULS results for all load combinations and checks
            TimberFrameULSResult result = element.ULSUtilization();
            
            //Retrieve the SLS results for all displacements combinations and checks
            TimberFrameSLSResult result = element.ULSUtilization();

            /// extract results 
            List<double[]> UtilsY = result.UtilsY;

            for (int i=0; i < UtilsY.Count; i++)
            {
                // Utils0 values for all load combinations 
                Console.WriteLine(UtilsY[i][0]);
            }
        }
```

You can also physically define an entire `TimberFrame` object which contains a sequence of the discretized analysed TimberFramePoints of the given frame.


### 🛠️ Prerequisites
You will need the following libraries and/or software installed before getting to the fun!
* [Rhino 7.21+](https://www.rhino3d.com/download/)
* [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)


<!-- CONTRIBUTING -->
## ⛑️ Contributing
You can contribute to Beaver by either contributing to it's **Functionality**, **CI/CD** and **Documentation**.
**CI/CD** and **Documentation** are the prioritites for now since they are currently just the basics (or not even that).
We are adding issues to the repo with the tags `Easy`, `Medium` and `Hard` so you can chose one with your wish of complexity to create a PR for it.
Feel free to add new issues and also contributing with your ideas in a correspoding PR.

### 🪵 Functionality
- [ ] Model
  - [X] Actions (Forces/Displacement) Model
  - [X] Combinations Model
  - [X] Section Model
  - [X] Connections Model
  - [ ] GlobalModel containing sections, connections and corresponding connectivities
  - [ ] Integration with section and connection forces
  - [ ] Addition of new structural design codes (and it does not have to be necessarily timber!)

### 📈 CI/CD
- [ ] CI
    - [ ] Unit tests in BeaverTests
    - [ ] Github actions for automatic tests run in each PR
- [ ] CD
    - [X]  Github actions for deployment of a NuGet package on in every `release` branch commit.
    - [ ]  Github actions to enable automatic versioning of NuGet releases.

### 📖 Documentation
- [ ] .NET documentation in every Beaver class, struct, enum, method and more you can imagine
- [ ] A really nice Wiki detailing the functionality of the API. While the code documentation describes the methods and reference them to the considered design check, this wiki should describe the design checks and then reference to the corresponding methods and objects that handle them.
- [ ] BeaverEducation project: This is a future project in our mind to use our documentation and API to create a timber structural design course, by direct use of the API for who's also learning how to code and the use of BeaverGrasshopper plugin for who's learning computational design with Grasshopper.

<!-- LICENSE -->
## 📋 License
Distributed under the MIT License. See `LICENSE.txt` for more information.
<p align="right">(<a href="#readme-top">back to top</a>)</p>

