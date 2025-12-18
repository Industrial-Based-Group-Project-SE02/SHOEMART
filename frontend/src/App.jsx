import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';

import Home from './pages/home';






function App() {

  return (
    <>
    
      <Router>

        <Toaster position='top-center'/>
        
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/client-page/*" element={<ClientPage />} />
          <Route path="/login" element={<Login  />} />
          
        </Routes>
      </Router>


     
    </>
  )
}

export default App