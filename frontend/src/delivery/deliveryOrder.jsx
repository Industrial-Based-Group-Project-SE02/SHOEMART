import { useEffect, useState } from "react";
import axios from "axios";
import toast from "react-hot-toast";
import { 
    Package, 
    Eye, 
    X, 
    Truck, 
    CheckCircle, 
    Phone,
    MapPin,
    Loader2,
    RefreshCw,
    ChevronLeft,
    ChevronRight,
    Calendar,
    User,
    AlertCircle,
    TrendingUp
} from "lucide-react";

export default function DeliveryOrder() {
    const [orders, setOrders] = useState([]);
    const [loading, setLoading] = useState(true);
    const [selectedOrder, setSelectedOrder] = useState(null);
    const [completingId, setCompletingId] = useState(null);
    const [currentPage, setCurrentPage] = useState(1);
    const itemsPerPage = 8;

    // Load Orders
    useEffect(() => {
        loadOrders();
    }, []);

    async function loadOrders() {
        setLoading(true);
        try {
            const token = localStorage.getItem("token");
            const res = await axios.get(
                "http://localhost:3000/api/orders/delivry_orders",
                { headers: { Authorization: `Bearer ${token}` } }
            );
            
            // Filter only delivering orders
            const deliveringOrders = res.data.filter(order => order.status === "delivering");
            setOrders(deliveringOrders);
        } catch (err) {
            toast.error("Failed to load orders");
            console.log(err);
        } finally {
            setLoading(false);
        }
    }

    // Complete Order
    async function acceptOrder(orderId) {
        setCompletingId(orderId);
        try {
            const token = localStorage.getItem("token");
            await axios.put(
                `http://localhost:3000/api/orders/complete_order/${orderId}`,
                {},
                { headers: { Authorization: `Bearer ${token}` } }
            );
            toast.success("Order marked as completed");
            loadOrders();

            if (selectedOrder?.order_id === orderId) {
                setSelectedOrder(null);
            }
        } catch (error) {
            toast.error(error.response?.data?.message || "Error updating order");
        } finally {
            setCompletingId(null);
        }
    }

    // Helper
    const formatDate = (date) => date ? new Date(date).toLocaleDateString('en-US', { 
        month: 'short', day: 'numeric', year: 'numeric' 
    }) : "N/A";

    const formatTime = (date) => date ? new Date(date).toLocaleTimeString('en-US', { 
        hour: '2-digit', minute: '2-digit'
    }) : "";

    const formatCurrency = (amount) => `Rs. ${Number(amount).toLocaleString()}`;

    // Pagination
    const totalPages = Math.ceil(orders.length / itemsPerPage);
    const paginatedOrders = orders.slice(
        (currentPage - 1) * itemsPerPage, 
        currentPage * itemsPerPage
    );

    return (
        <div className="h-full flex flex-col overflow-hidden bg-gray-50">

            {/* ============ HEADER ============ */}
            <div className="flex-shrink-0 bg-gradient-to-r from-black to-gray-900 px-4 sm:px-6 py-4 border-b-4 border-red-600">
                <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 sm:w-12 sm:h-12 bg-gradient-to-br from-red-600 to-red-700 rounded-lg flex items-center justify-center shadow-lg">
                            <Truck className="w-5 h-5 sm:w-6 sm:h-6 text-white" />
                        </div>
                        <div>
                            <h1 className="text-xl sm:text-2xl font-black text-white">ACTIVE DELIVERIES</h1>
                            <p className="text-xs sm:text-sm text-gray-400">
                                {loading ? "Loading..." : `${orders.length} ${orders.length === 1 ? 'order' : 'orders'} in transit`}
                            </p>
                        </div>
                    </div>
                    <button 
                        onClick={loadOrders}
                        disabled={loading}
                        className="w-10 h-10 bg-white/10 hover:bg-white/20 rounded-lg text-white flex items-center justify-center transition-all disabled:opacity-50"
                    >
                        <RefreshCw className={`w-5 h-5 ${loading ? "animate-spin" : ""}`} />
                    </button>
                </div>
            </div>

            {/* ============ STATS BAR ============ */}
            <div className="flex-shrink-0 bg-white border-b-2 border-gray-200 px-4 sm:px-6 py-4">
                <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                    {/* Active Deliveries */}
                    <div className="bg-gradient-to-br from-red-50 to-red-100 rounded-xl p-4 border-2 border-red-200 hover:border-red-600 transition-all">
                        <div className="flex items-center justify-between mb-2">
                            <Truck className="w-8 h-8 text-red-600" />
                            <span className="text-3xl font-black text-red-900">
                                {loading ? "-" : orders.length}
                            </span>
                        </div>
                        <p className="text-xs font-bold text-red-700 uppercase">Active Deliveries</p>
                    </div>

                    {/* Pending Completion */}
                    <div className="bg-gradient-to-br from-gray-50 to-gray-100 rounded-xl p-4 border-2 border-gray-200 hover:border-black transition-all">
                        <div className="flex items-center justify-between mb-2">
                            <Package className="w-8 h-8 text-black" />
                            <span className="text-3xl font-black text-black">
                                {loading ? "-" : orders.length}
                            </span>
                        </div>
                        <p className="text-xs font-bold text-gray-700 uppercase">Pending Completion</p>
                    </div>

                    {/* Total Value */}
                    <div className="bg-gradient-to-br from-red-50 to-red-100 rounded-xl p-4 border-2 border-red-200 hover:border-red-600 transition-all">
                        <div className="flex items-center justify-between mb-2">
                            <TrendingUp className="w-8 h-8 text-red-600" />
                            <span className="text-2xl font-black text-red-900">
                                {loading ? "-" : `₨${((orders.reduce((sum, o) => sum + Number(o.total), 0)) / 1000).toFixed(1)}k`}
                            </span>
                        </div>
                        <p className="text-xs font-bold text-red-700 uppercase">Total Value</p>
                    </div>
                </div>
            </div>

            {/* ============ CONTENT ============ */}
            <div className="flex-1 overflow-hidden bg-white">
                
                {/* Loading */}
                {loading ? (
                    <div className="h-full flex items-center justify-center">
                        <div className="text-center">
                            <Loader2 className="w-10 h-10 text-red-600 animate-spin mx-auto mb-4" />
                            <p className="text-gray-600 font-semibold">Loading deliveries...</p>
                        </div>
                    </div>
                
                /* Empty State */
                ) : orders.length === 0 ? (
                    <div className="h-full flex flex-col items-center justify-center p-6">
                        <div className="w-20 h-20 bg-red-100 rounded-full flex items-center justify-center mb-4">
                            <Truck className="w-10 h-10 text-red-600" />
                        </div>
                        <h3 className="font-black text-xl mb-2 text-gray-900">All Caught Up!</h3>
                        <p className="text-gray-500 text-center max-w-sm">
                            No active deliveries at the moment. Check back soon for new orders.
                        </p>
                    </div>
                
                /* Orders List */
                ) : (
                    <div className="h-full flex flex-col">
                        
                        {/* Table Header - Desktop */}
                        <div className="flex-shrink-0 bg-gray-50 border-b-2 border-gray-200 px-4 py-3 hidden sm:grid grid-cols-12 gap-4 text-xs font-bold text-gray-600 uppercase">
                            <div className="col-span-1">Order ID</div>
                            <div className="col-span-3">Customer</div>
                            <div className="col-span-3">Delivery Address</div>
                            <div className="col-span-2">Amount</div>
                            <div className="col-span-1">Items</div>
                            <div className="col-span-2 text-center">Actions</div>
                        </div>

                        {/* Orders */}
                        <div className="flex-1 overflow-y-auto">
                            {paginatedOrders.map((order, index) => (
                                <div 
                                    key={order.order_id} 
                                    className="border-b border-gray-100 hover:bg-red-50 transition-all"
                                    style={{ animationDelay: `${index * 50}ms` }}
                                >
                                    
                                    {/* Mobile View */}
                                    <div className="sm:hidden p-4">
                                        <div className="flex items-start justify-between mb-3">
                                            <div>
                                                <div className="flex items-center gap-2 mb-1">
                                                    <span className="font-black text-gray-900">#{order.order_id}</span>
                                                    <span className="px-2 py-0.5 bg-red-100 text-red-700 text-xs font-bold rounded-full border border-red-200">
                                                        🚚 Delivering
                                                    </span>
                                                </div>
                                                <p className="font-bold text-sm">{order.customer_name}</p>
                                                <p className="text-xs text-gray-500 flex items-center gap-1 mt-1">
                                                    <Phone className="w-3 h-3" />
                                                    {order.customer_phone}
                                                </p>
                                            </div>
                                        </div>

                                        <div className="bg-gray-50 rounded-lg p-3 mb-3 border border-gray-200">
                                            <p className="text-xs text-gray-600 flex items-start gap-2">
                                                <MapPin className="w-4 h-4 text-red-600 flex-shrink-0 mt-0.5" />
                                                {order.customer_address}
                                            </p>
                                        </div>

                                        <div className="flex items-center justify-between">
                                            <div>
                                                <p className="text-xs text-gray-500">Collection Amount</p>
                                                <p className="font-black text-lg text-red-600">
                                                    {formatCurrency(order.total)}
                                                </p>
                                            </div>
                                            <div className="flex gap-2">
                                                <button
                                                    onClick={() => setSelectedOrder(order)}
                                                    className="px-3 py-1.5 bg-black text-white text-xs font-bold rounded-lg hover:bg-gray-800 transition-all shadow-md"
                                                >
                                                    View
                                                </button>
                                                <button
                                                    onClick={() => acceptOrder(order.order_id)}
                                                    disabled={completingId === order.order_id}
                                                    className="px-3 py-1.5 bg-gradient-to-r from-red-600 to-red-700 text-white text-xs font-bold rounded-lg flex items-center gap-1 disabled:opacity-50 shadow-md"
                                                >
                                                    {completingId === order.order_id ? (
                                                        <Loader2 className="w-3 h-3 animate-spin" />
                                                    ) : (
                                                        <>
                                                            <CheckCircle className="w-3 h-3" />
                                                            Complete
                                                        </>
                                                    )}
                                                </button>
                                            </div>
                                        </div>
                                    </div>

                                    {/* Desktop View */}
                                    <div className="hidden sm:grid grid-cols-12 gap-4 px-4 py-4 items-center">
                                        <div className="col-span-1">
                                            <span className="font-black text-gray-900">#{order.order_id}</span>
                                        </div>
                                        <div className="col-span-3">
                                            <p className="font-bold text-sm text-gray-900">{order.customer_name}</p>
                                            <p className="text-xs text-gray-500 flex items-center gap-1 mt-0.5">
                                                <Phone className="w-3 h-3" />
                                                {order.customer_phone}
                                            </p>
                                        </div>
                                        <div className="col-span-3">
                                            <p className="text-sm text-gray-600 line-clamp-2 flex items-start gap-1">
                                                <MapPin className="w-4 h-4 flex-shrink-0 mt-0.5 text-red-600" />
                                                {order.customer_address}
                                            </p>
                                        </div>
                                        <div className="col-span-2">
                                            <p className="font-black text-red-600">
                                                {formatCurrency(order.total)}
                                            </p>
                                        </div>
                                        <div className="col-span-1">
                                            <span className="text-sm text-gray-600 font-semibold">
                                                {order.items?.length || 0}
                                            </span>
                                        </div>
                                        <div className="col-span-2 flex gap-2 justify-center">
                                            <button
                                                onClick={() => setSelectedOrder(order)}
                                                className="px-3 py-1.5 bg-gray-100 hover:bg-black hover:text-white text-sm font-bold rounded-lg transition-all"
                                            >
                                                View
                                            </button>
                                            <button
                                                onClick={() => acceptOrder(order.order_id)}
                                                disabled={completingId === order.order_id}
                                                className="px-3 py-1.5 bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 text-white text-sm font-bold rounded-lg flex items-center gap-1 transition-all disabled:opacity-50 shadow-md"
                                            >
                                                {completingId === order.order_id ? (
                                                    <Loader2 className="w-4 h-4 animate-spin" />
                                                ) : (
                                                    <>
                                                        <CheckCircle className="w-4 h-4" />
                                                        Complete
                                                    </>
                                                )}
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>

                        {/* Pagination */}
                        {totalPages > 1 && (
                            <div className="flex-shrink-0 border-t-2 border-gray-200 px-4 py-3 flex items-center justify-between bg-gray-50">
                                <p className="text-sm text-gray-600">
                                    Showing <span className="font-bold text-black">{(currentPage - 1) * itemsPerPage + 1}</span> to{" "}
                                    <span className="font-bold text-black">{Math.min(currentPage * itemsPerPage, orders.length)}</span> of{" "}
                                    <span className="font-bold text-black">{orders.length}</span> deliveries
                                </p>
                                <div className="flex items-center gap-2">
                                    <button
                                        onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                                        disabled={currentPage === 1}
                                        className="px-3 py-1.5 bg-white border-2 border-gray-300 rounded-lg text-sm font-bold disabled:opacity-50 hover:border-black transition-all"
                                    >
                                        Previous
                                    </button>
                                    <span className="px-3 py-1.5 text-sm font-bold">
                                        Page {currentPage} of {totalPages}
                                    </span>
                                    <button
                                        onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                                        disabled={currentPage === totalPages}
                                        className="px-3 py-1.5 bg-white border-2 border-gray-300 rounded-lg text-sm font-bold disabled:opacity-50 hover:border-black transition-all"
                                    >
                                        Next
                                    </button>
                                </div>
                            </div>
                        )}
                    </div>
                )}
            </div>

            {/* ============ ORDER DETAIL MODAL ============ */}
            {selectedOrder && (
                <OrderDetailModal 
                    order={selectedOrder}
                    onClose={() => setSelectedOrder(null)}
                    onComplete={acceptOrder}
                    completingId={completingId}
                    formatDate={formatDate}
                    formatTime={formatTime}
                    formatCurrency={formatCurrency}
                />
            )}
        </div>
    );
}


// ============ ORDER DETAIL MODAL ============
function OrderDetailModal({ order, onClose, onComplete, completingId, formatDate, formatTime, formatCurrency }) {

    return (
        <>
            {/* Backdrop */}
            <div className="fixed inset-0 z-40 bg-black/50 backdrop-blur-sm" onClick={onClose} />

            {/* Modal */}
            <div className="fixed inset-0 z-50 flex items-end sm:items-center justify-center p-0 sm:p-4">
                <div 
                    className="relative w-full sm:max-w-2xl max-h-[90vh] bg-white flex flex-col overflow-hidden rounded-t-3xl sm:rounded-2xl shadow-2xl border-4 border-red-600"
                    onClick={(e) => e.stopPropagation()}
                >
                    
                    {/* Header */}
                    <div className="bg-gradient-to-r from-red-600 to-red-700 text-white p-6">
                        <div className="flex items-center justify-between mb-4">
                            <div className="flex items-center gap-3">
                                <div className="w-12 h-12 bg-white/20 rounded-lg flex items-center justify-center shadow-lg">
                                    <Truck className="w-6 h-6" />
                                </div>
                                <div>
                                    <h2 className="text-2xl font-black">ORDER #{order.order_id}</h2>
                                    <div className="flex items-center gap-3 text-sm text-red-100 mt-1">
                                        <span>{formatDate(order.created_at)}</span>
                                        <span>•</span>
                                        <span>{formatTime(order.created_at)}</span>
                                    </div>
                                </div>
                            </div>
                            <button 
                                onClick={onClose}
                                className="w-10 h-10 bg-white/10 hover:bg-white/20 rounded-lg flex items-center justify-center transition-all"
                            >
                                <X className="w-5 h-5" />
                            </button>
                        </div>

                        <div className="inline-flex items-center gap-2 px-4 py-2 bg-white/20 rounded-lg font-bold">
                            <Truck className="w-5 h-5" />
                            Out for Delivery
                        </div>
                    </div>

                    {/* Content */}
                    <div className="flex-1 overflow-y-auto p-6 space-y-4">
                        
                        {/* Customer Info Card */}
                        <div className="bg-gradient-to-br from-gray-50 to-gray-100 rounded-xl p-5 border-2 border-gray-200">
                            <h4 className="font-bold text-sm text-gray-900 uppercase mb-4 flex items-center gap-2">
                                <User className="w-4 h-4 text-red-600" />
                                Delivery Information
                            </h4>
                            <div className="space-y-3">
                                <div>
                                    <p className="text-xs text-gray-500 mb-1 font-semibold">Customer Name</p>
                                    <p className="font-bold text-gray-900">{order.customer_name}</p>
                                </div>
                                <div>
                                    <p className="text-xs text-gray-500 mb-1 font-semibold">Phone Number</p>
                                    <p className="font-bold text-gray-900 flex items-center gap-2">
                                        <Phone className="w-4 h-4 text-gray-400" />
                                        {order.customer_phone}
                                    </p>
                                </div>
                                <div>
                                    <p className="text-xs text-gray-500 mb-1 font-semibold">Delivery Address</p>
                                    <p className="font-bold text-gray-900 flex items-start gap-2">
                                        <MapPin className="w-4 h-4 text-red-600 flex-shrink-0 mt-0.5" />
                                        {order.customer_address}
                                    </p>
                                </div>
                            </div>
                        </div>

                        {/* Items */}
                        <div>
                            <h4 className="font-bold text-sm text-gray-900 uppercase mb-4 flex items-center gap-2">
                                <Package className="w-4 h-4 text-red-600" />
                                Order Items ({order.items?.length || 0})
                            </h4>
                            <div className="space-y-3">
                                {order.items?.map((item, idx) => (
                                    <div key={idx} className="bg-white rounded-lg border-2 border-gray-200 p-4 hover:border-red-600 hover:shadow-lg transition-all">
                                        <div className="flex gap-4">
                                            <img 
                                                src={item.image} 
                                                alt={item.product_name}
                                                className="w-20 h-20 object-cover rounded-lg bg-gray-100 border-2 border-gray-200"
                                            />
                                            <div className="flex-1">
                                                <p className="font-bold text-gray-900 mb-2">{item.product_name}</p>
                                                <div className="flex gap-2 mb-2">
                                                    <span className="px-2 py-1 bg-black text-white text-xs font-bold rounded">
                                                        SIZE {item.size_value}
                                                    </span>
                                                    <span className="px-2 py-1 bg-gray-100 border border-gray-300 text-xs font-bold rounded">
                                                        QTY: {item.quantity}
                                                    </span>
                                                </div>
                                                <p className="font-black text-red-600">
                                                    {formatCurrency(item.line_total)}
                                                </p>
                                            </div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>

                        {/* Payment Summary */}
                        <div className="bg-gradient-to-br from-black to-gray-900 text-white rounded-xl p-5 border-4 border-red-600">
                            <h4 className="font-black text-sm uppercase mb-4">Payment Collection</h4>
                            <div className="space-y-3">
                                <div className="flex justify-between">
                                    <span className="text-gray-400">Order Total</span>
                                    <span className="font-bold">{formatCurrency(order.total)}</span>
                                </div>
                                <div className="flex justify-between">
                                    <span className="text-gray-400">Delivery Fee</span>
                                    <span className="text-red-400 font-bold">FREE</span>
                                </div>
                                <div className="border-t-2 border-red-600/50 pt-3 flex justify-between items-center">
                                    <span className="text-lg font-black">Collect Cash</span>
                                    <span className="text-2xl font-black text-red-500">
                                        {formatCurrency(order.total)}
                                    </span>
                                </div>
                            </div>
                            <div className="mt-4 pt-4 border-t-2 border-red-600/50">
                                <p className="text-xs text-gray-400">💰 Cash on Delivery - Collect exact amount</p>
                            </div>
                        </div>
                    </div>

                    {/* Footer */}
                    <div className="border-t-2 border-gray-200 p-4 bg-gray-50 flex gap-3">
                        <button
                            onClick={() => onComplete(order.order_id)}
                            disabled={completingId === order.order_id}
                            className="flex-1 h-12 bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 text-white font-black rounded-lg flex items-center justify-center gap-2 transition-all disabled:opacity-50 shadow-lg"
                        >
                            {completingId === order.order_id ? (
                                <>
                                    <Loader2 className="w-5 h-5 animate-spin" />
                                    COMPLETING...
                                </>
                            ) : (
                                <>
                                    <CheckCircle className="w-5 h-5" />
                                    MARK AS DELIVERED
                                </>
                            )}
                        </button>
                        <button
                            onClick={onClose}
                            className="px-6 h-12 bg-gradient-to-r from-black to-gray-900 hover:from-gray-900 hover:to-black text-white font-black rounded-lg transition-all shadow-lg"
                        >
                            CLOSE
                        </button>
                    </div>
                </div>
            </div>
        </>
    );
}